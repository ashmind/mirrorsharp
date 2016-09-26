using System.Collections.Immutable;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using MirrorSharp.Internal;
using MirrorSharp.Internal.Commands;

namespace MirrorSharp.Advanced {
    public abstract class MiddlewareBase {
        private ImmutableArray<ICommandHandler> _commands;

        private readonly MirrorSharpOptions _options;

        protected MiddlewareBase(MirrorSharpOptions options) {
            _options = options;
            _commands = CreateCommands();
        }

        private ImmutableArray<ICommandHandler> CreateCommands() {
            var commands = new ICommandHandler[26];
            foreach (var command in new ICommandHandler[] {
                new CommitCompletionHandler(),
                new MoveCursorHandler(),
                new ReplaceTextHandler(),
                new SlowUpdateHandler(),
                new TypeCharHandler()
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
