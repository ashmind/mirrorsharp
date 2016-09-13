using System;
using System.Net.WebSockets;
using System.Threading.Tasks;
using MirrorSharp.Internal;

namespace MirrorSharp.Advanced {
    public abstract class MiddlewareBase {
        private readonly MirrorSharpOptions _options;

        protected MiddlewareBase(MirrorSharpOptions options) {
            _options = options;
        }

        protected async Task WebSocketLoopAsync(WebSocket socket) {
            WorkSession session = null;
            Connection connection = null;
            try {
                session = new WorkSession();
                connection = new Connection(socket, session, _options);

                while (connection.IsConnected) {
                    try {
                        await connection.ReceiveAndProcessAsync();
                    }
                    catch {
                        // this is sent back by connection itself
                    }
                }
            }
            catch (Exception) when (connection == null && session != null) {
                await session.DisposeAsync().ConfigureAwait(false);
                throw;
            }
            finally {
                if (connection != null)
                    await connection.DisposeAsync().ConfigureAwait(false);
            }
        }
    }
}
