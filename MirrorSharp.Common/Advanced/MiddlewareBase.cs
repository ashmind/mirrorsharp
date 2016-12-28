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

        protected MiddlewareBase([CanBeNull] MirrorSharpOptions options) {
            _options = options;
            _languages = new[] {new CSharpLanguage()};
            _handlers = CreateHandlersIndexedByCommandId();
        }

        [ItemNotNull]
        private ImmutableArray<ICommandHandler> CreateHandlersIndexedByCommandId() {
            var handlers = new ICommandHandler[26];
            foreach (var handler in CreateHandlers()) {
                foreach (var id in handler.CommandIds) {
                    handlers[id - 'A'] = handler;
                }
            }
            return ImmutableArray.CreateRange(handlers);
        }

        [NotNull, ItemNotNull]
        protected IReadOnlyCollection<ICommandHandler> CreateHandlers() {
            var completion = new CompletionSupport();
            var signatureHelp = new SignatureHelpSupport();
            return new ICommandHandler[] {
                new ApplyDiagnosticActionHandler(),
                new CompletionStateHandler(completion),
                new MoveCursorHandler(signatureHelp),
                new ReplaceTextHandler(signatureHelp, completion),
                new RequestSelfDebugDataHandler(),
                new SetOptionsHandler(_languages, _options?.SetOptionsFromClient),
                new SlowUpdateHandler(_options?.SlowUpdate),
                new TypeCharHandler(completion, signatureHelp)
            };
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
