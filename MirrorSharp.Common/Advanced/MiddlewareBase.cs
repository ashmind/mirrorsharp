using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using MirrorSharp.Internal;
using MirrorSharp.Internal.Handlers;
using MirrorSharp.Internal.Handlers.Shared;
using MirrorSharp.Internal.Languages;

namespace MirrorSharp.Advanced {
    public abstract class MiddlewareBase {
        [CanBeNull] private readonly MirrorSharpOptions _options;
        [NotNull, ItemNotNull] private readonly IReadOnlyCollection<ILanguage> _languages;
        [ItemNotNull] private readonly ImmutableArray<ICommandHandler> _handlers;

        protected MiddlewareBase([CanBeNull] MirrorSharpOptions options) : this(new CSharpLanguage(), new VisualBasicLanguage(), options) {
        }

        internal MiddlewareBase([NotNull] CSharpLanguage csharp, [NotNull] VisualBasicLanguage visualBasic, [CanBeNull] MirrorSharpOptions options) {
            _options = options;
            _languages = new ILanguage[] { csharp, visualBasic };
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
        internal IReadOnlyCollection<ICommandHandler> CreateHandlers() {
            var completion = new CompletionSupport();
            var signatureHelp = new SignatureHelpSupport();
            var typedCharEffects = new TypedCharEffects(completion, signatureHelp);
            return new ICommandHandler[] {
                new ApplyDiagnosticActionHandler(),
                new CompletionStateHandler(completion),
                new MoveCursorHandler(signatureHelp),
                new ReplaceTextHandler(signatureHelp, completion, typedCharEffects),
                new RequestSelfDebugDataHandler(),
                new SetOptionsHandler(_languages, _options?.SetOptionsFromClient),
                new SignatureHelpStateHandler(signatureHelp),
                new SlowUpdateHandler(_options?.SlowUpdate),
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
                session = new WorkSession(_languages.OfType<CSharpLanguage>().First(), _options);
                connection = new Connection(socket, session, _handlers, _options);

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
