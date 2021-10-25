using System.Buffers;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using MirrorSharp.Internal.Handlers;
using MirrorSharp.Internal.Handlers.Shared;

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

        protected async Task WebSocketLoopAsync(WebSocket socket, CancellationToken cancellationToken) {
            WorkSession? session = null;
            Connection? connection = null;
            try {
                session = new WorkSession(_languageManager.GetLanguage(LanguageNames.CSharp), _options, _extensions);
                connection = new Connection(socket, session, _handlers, ArrayPool<byte>.Shared, _extensions.ConnectionSendViewer, _extensions.ExceptionLogger, _options);

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
                    session?.Dispose();
                }
            }
        }
    }
}
