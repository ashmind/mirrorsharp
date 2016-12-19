using System.Collections.Immutable;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using MirrorSharp.Internal;
using MirrorSharp.Internal.Handlers;
using MirrorSharp.Internal.Handlers.Shared;

namespace MirrorSharp.Advanced {
    public abstract class MiddlewareBase {
        private readonly ImmutableArray<ICommandHandler> _commands;
        private readonly MirrorSharpOptions _options;

        protected MiddlewareBase(MirrorSharpOptions options) {
            _options = options;
            _commands = CreateCommands();
        }

        protected virtual ImmutableArray<ICommandHandler> CreateCommands() {
            var commands = new ICommandHandler[26];
            var signatureHelp = new SignatureHelpSupport();
            foreach (var command in new ICommandHandler[] {
                new ApplyDiagnosticActionHandler(),
                new CompletionChoiceHandler(),
                new MoveCursorHandler(signatureHelp),
                new ReplaceTextHandler(signatureHelp),
                new SlowUpdateHandler(),
                new TypeCharHandler(signatureHelp)
            }) {
                foreach (var id in command.CommandIds) {
                    commands[id - 'A'] = command;
                }
            }
            return ImmutableArray.CreateRange(commands);
        }

        protected async Task WebSocketLoopAsync(WebSocket socket, CancellationToken cancellationToken) {
            WorkSession session = null;
            Connection connection = null;
            try {
                session = new WorkSession();
                connection = new Connection(socket, session, _commands, _options);

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
