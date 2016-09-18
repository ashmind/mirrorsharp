using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using MirrorSharp.Internal;

namespace MirrorSharp.Advanced {
    public abstract class MiddlewareBase {
        private readonly MirrorSharpOptions _options;

        protected MiddlewareBase(MirrorSharpOptions options) {
            _options = options;
        }

        protected async Task WebSocketLoopAsync(WebSocket socket, CancellationToken cancellationToken) {
            WorkSession session = null;
            Connection connection = null;
            try {
                session = new WorkSession();
                connection = new Connection(socket, session, _options);

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
