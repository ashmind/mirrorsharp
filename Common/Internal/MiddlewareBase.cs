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
        private readonly MirrorSharpOptions _options;
        private readonly ImmutableArray<ICommandHandler> _handlers;

        protected MiddlewareBase(MirrorSharpOptions options) 
            : this(new LanguageManager(Argument.NotNull(nameof(options), options)), options) {
        }

        internal MiddlewareBase(LanguageManager languageManager, MirrorSharpOptions options) {
            _options = options;
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
                new SetOptionsHandler(_languageManager, ArrayPool<char>.Shared, _options.SetOptionsFromClient),
                new SignatureHelpStateHandler(signatureHelp),
                new RequestInfoTipHandler(),
                new SlowUpdateHandler(_options.SlowUpdate),
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
