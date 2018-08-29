using System.Buffers;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using MirrorSharp.Internal.Handlers;
using MirrorSharp.Internal.Handlers.Shared;

namespace MirrorSharp.Internal {
    internal abstract class MiddlewareBase {
        [NotNull] private readonly LanguageManager _languageManager;
        [NotNull] private readonly MirrorSharpOptions _options;
        [ItemNotNull] private readonly ImmutableArray<ICommandHandler> _handlers;

        protected MiddlewareBase([NotNull] MirrorSharpOptions options) 
            : this(new LanguageManager(Argument.NotNull(nameof(options), options)), options) {
        }

        internal MiddlewareBase([NotNull] LanguageManager languageManager, [NotNull] MirrorSharpOptions options) {
            _options = options;
            _languageManager = languageManager;
            _handlers = CreateHandlersIndexedByCommandId();
        }

        [ItemNotNull]
        private ImmutableArray<ICommandHandler> CreateHandlersIndexedByCommandId() {
            var handlers = new ICommandHandler[26];
            foreach (var handler in CreateHandlers()) {
                handlers[handler.CommandId - 'A'] = handler;
            }
            return ImmutableArray.CreateRange(handlers);
        }

        [NotNull, ItemNotNull]
        private IReadOnlyCollection<ICommandHandler> CreateHandlers() {
            var completion = new CompletionSupport();
            var signatureHelp = new SignatureHelpSupport();
            var typedCharEffects = new TypedCharEffects(completion, signatureHelp);
            return new ICommandHandler[] {
                new ApplyDiagnosticActionHandler(),
                new CompletionStateHandler(completion),
                new ExtensionCommandHandler((IReadOnlyCollection<ICommandExtension>)_options.Extensions),
                new MoveCursorHandler(signatureHelp),
                new ReplaceTextHandler(signatureHelp, completion, typedCharEffects, ArrayPool<char>.Shared),
                new RequestSelfDebugDataHandler(),
                new SetOptionsHandler(_languageManager, ArrayPool<char>.Shared, _options.SetOptionsFromClient),
                new SignatureHelpStateHandler(signatureHelp),
                new SlowUpdateHandler(_options.SlowUpdate),
                new TypeCharHandler(typedCharEffects)
            };
        }

        internal ICommandHandler GetHandler(char commandId) {
            return _handlers[commandId - 'A'];
        }

        [NotNull]
        protected async Task WebSocketLoopAsync([NotNull] WebSocket socket, CancellationToken cancellationToken) {
            WorkSession session = null;
            Connection connection = null;
            try {
                session = new WorkSession(_languageManager.GetLanguage(LanguageNames.CSharp), _options);
                connection = new Connection(socket, session, _handlers, ArrayPool<byte>.Shared, _options);

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
