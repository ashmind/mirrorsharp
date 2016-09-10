using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;

namespace MirrorSharp.Internal {
    public class Middleware {
        private readonly RequestDelegate _next;

        public Middleware(RequestDelegate next) {
            _next = next;
        }

        [UsedImplicitly]
        public async Task Invoke(HttpContext context) {
            if (!context.WebSockets.IsWebSocketRequest) {
                await _next(context);
                return;
            }

            using (var socket = await context.WebSockets.AcceptWebSocketAsync().ConfigureAwait(false)) {
                var output = new byte[2048];

                WorkSession session = null;
                Connection connection = null;
                try {
                    session = new WorkSession();
                    connection = new Connection(socket, session);

                    while (connection.IsConnected) {
                        try {
                            await connection.ReceiveAndProcessAsync();
                        }
                        catch {
                            // this is sent back by connection itself
                        }
                    }
                }
                catch (Exception) when(connection == null && session != null) {
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
}
