using System;
using System.Buffers;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using MirrorSharp.Internal.Handlers;
using MirrorSharp.Internal.Handlers.Shared;
using MirrorSharp.Internal.Results;

namespace MirrorSharp.Internal {
    internal abstract class MiddlewareBase {
        private readonly LanguageManager _languageManager;
        private readonly IMiddlewareOptions _options;
        private readonly ImmutableExtensionServices _extensions;
        private readonly ImmutableArray<ICommandHandler> _handlers;

        protected MiddlewareBase(IMiddlewareOptions options, ImmutableExtensionServices extensions)
            : this(new LanguageManager(Argument.NotNull(nameof(options), options)), options, extensions) {
        }

        internal MiddlewareBase(LanguageManager languageManager, IMiddlewareOptions options, ImmutableExtensionServices extensions) {
            _options = options;
            _extensions = extensions;
            _languageManager = languageManager;
            _handlers = CreateHandlersIndexedByCommandId();
        }

        private ImmutableArray<ICommandHandler> CreateHandlersIndexedByCommandId() {
            var handlers = new ICommandHandler[26];
            foreach (var handler in CreateHandlers()) {
                handlers[handler.CommandId - 'A'] = handler;
            }
            return ImmutableArray.CreateRange(handlers);
        }

        private IReadOnlyCollection<ICommandHandler> CreateHandlers() {
            var completion = new CompletionSupport();
            var signatureHelp = new SignatureHelpSupport();
            var typedCharEffects = new TypedCharEffects(completion, signatureHelp);
            return new ICommandHandler[] {
                new ApplyDiagnosticActionHandler(),
                new CompletionStateHandler(completion),
                new MoveCursorHandler(signatureHelp),
                new ReplaceTextHandler(signatureHelp, completion, typedCharEffects, ArrayPool<char>.Shared),
                new RequestSelfDebugDataHandler(),
                #pragma warning disable CS0618 // Type or member is obsolete
                new SetOptionsHandler(_languageManager, ArrayPool<char>.Shared, _extensions.SetOptionsFromClient),
                #pragma warning restore CS0618
                new SignatureHelpStateHandler(signatureHelp),
                new RequestInfoTipHandler(),
                #pragma warning disable CS0618 // Type or member is obsolete
                new SlowUpdateHandler(_extensions.SlowUpdate),
                #pragma warning restore CS0618 
                new TypeCharHandler(typedCharEffects)
            };
        }

        internal ICommandHandler GetHandler(char commandId) {
            return _handlers[commandId - 'A'];
        }

        protected async Task SlowTestStatusAsync(CancellationToken cancellationToken) {
            using var sender = new NullCommandResultSender(ArrayPool<byte>.Shared);
            using var session = StartWorkSession();

            Task ExecuteAsync(char commandId, string command) {
                var byteCount = Encoding.UTF8.GetByteCount(command);
                byte[]? bytes = null;
                try {
                    bytes = ArrayPool<byte>.Shared.Rent(byteCount);
                    Encoding.UTF8.GetBytes(command, 0, command.Length, bytes, 0);
                    var asyncData = new AsyncData(bytes.AsMemory(0, byteCount), false, () => throw new());
                    return GetHandler(commandId).ExecuteAsync(asyncData, session, sender, cancellationToken);
                }
                finally {
                    if (bytes != null)
                        ArrayPool<byte>.Shared.Return(bytes);
                }
            }

            if (_options.StatusTestCommands.Count == 0)
                throw new NotSupportedException("TODO: Implement default command sequence before a NuGet release");

            foreach (var (commandId, commandText) in _options.StatusTestCommands) {
                await ExecuteAsync(commandId, commandText).ConfigureAwait(false);
            }
        }

        protected async Task WebSocketLoopAsync(WebSocket socket, CancellationToken cancellationToken) {
            WorkSession? session = null;
            FastUtf8JsonWriter? messageJsonWriter = null;
            ConnectionMessageWriter? messageWriter = null;
            IConnection? connection = null;
            try {
                messageJsonWriter = new FastUtf8JsonWriter(ArrayPool<byte>.Shared);
                messageWriter = new ConnectionMessageWriter(messageJsonWriter);
                try {
                    session = StartWorkSession();
                    connection = new Connection(socket, session, ArrayPool<byte>.Shared, _handlers, messageWriter, _extensions.ConnectionSendViewer, _extensions.ExceptionLogger, _options);
                }
                catch (Exception ex) {
                    connection = new StartupFailedConnection(socket, ex, ArrayPool<byte>.Shared, messageWriter, _options);
                }

                while (connection.IsConnected) {
                    try {
                        await connection.ReceiveAndProcessAsync(cancellationToken).ConfigureAwait(false);
                    }
                    catch {
                        // this is sent back by connection itself
                    }
                }
            }
            finally {
                if (connection != null) {
                    connection.Dispose();
                }
                else {
                    try {
                        if (messageWriter != null) {
                            messageWriter.Dispose();
                        }
                        else {
                            messageJsonWriter?.Dispose();
                        }
                    }
                    finally {
                        session?.Dispose();
                    }
                }
            }
        }

        private WorkSession StartWorkSession() {
            return new WorkSession(_languageManager.GetLanguage(LanguageNames.CSharp), _options, _extensions);
        }
    }
}
