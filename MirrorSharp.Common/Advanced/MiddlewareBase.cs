using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using MirrorSharp.Internal;
using MirrorSharp.Internal.Handlers;
using MirrorSharp.Internal.Handlers.Shared;
using MirrorSharp.Internal.Languages;

namespace MirrorSharp.Advanced {
    public abstract class MiddlewareBase {
        private readonly ImmutableArray<ICommandHandler> _handlers;
        private readonly MirrorSharpOptions _options;
        private readonly IReadOnlyCollection<ILanguage> _languages;

        protected MiddlewareBase(MirrorSharpOptions options) {
            _options = options;
            _languages = new[] {new CSharpLanguage()};
            _handlers = CreateHandlersIndexedByCommandId();
        }

        private ImmutableArray<ICommandHandler> CreateHandlersIndexedByCommandId() {
            var handlers = new ICommandHandler[26];
            foreach (var handler in CreateHandlers()) {
                foreach (var id in handler.CommandIds) {
                    handlers[id - 'A'] = handler;
                }
            }
            return ImmutableArray.CreateRange(handlers);
        }

        protected IReadOnlyCollection<ICommandHandler> CreateHandlers() {
            var signatureHelp = new SignatureHelpSupport();
            return new ICommandHandler[] {
                new ApplyDiagnosticActionHandler(),
                new CompletionChoiceHandler(),
                new MoveCursorHandler(signatureHelp),
                new ReplaceTextHandler(signatureHelp),
                new RequestSelfDebugDataHandler(),
                new SetOptionsHandler(_languages),
                new SlowUpdateHandler(_options.SlowUpdate),
                new TypeCharHandler(signatureHelp)
            };
        }

        protected async Task WebSocketLoopAsync(WebSocket socket, CancellationToken cancellationToken) {
            WorkSession session = null;
            Connection connection = null;
            try {
                session = new WorkSession(_languages.OfType<CSharpLanguage>().First(), _options.SelfDebugEnabled ? new SelfDebug() : null);
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
