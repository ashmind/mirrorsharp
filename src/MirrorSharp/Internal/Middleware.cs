using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;

namespace MirrorSharp.Internal {
    public class Middleware {
        private readonly RequestDelegate _next;
        private readonly MirrorSharpOptions _options;

        public Middleware([NotNull] RequestDelegate next, [CanBeNull] MirrorSharpOptions options) {
            _next = Argument.NotNull(nameof(next), next);
            _options = options;
        }

        [UsedImplicitly]
        public async Task Invoke(HttpContext context) {
            if (!context.WebSockets.IsWebSocketRequest) {
                await _next(context);
                return;
            }

            using (var socket = await context.WebSockets.AcceptWebSocketAsync().ConfigureAwait(false)) {
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
