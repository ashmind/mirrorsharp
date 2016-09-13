using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using MirrorSharp.Advanced;

namespace MirrorSharp.AspNetCore.Internal {
    internal class Middleware : MiddlewareBase {
        private readonly RequestDelegate _next;

        public Middleware([NotNull] RequestDelegate next, [CanBeNull] MirrorSharpOptions options) : base(options) {
            _next = Argument.NotNull(nameof(next), next);
        }

        [UsedImplicitly]
        public async Task Invoke(HttpContext context) {
            if (!context.WebSockets.IsWebSocketRequest) {
                await _next(context).ConfigureAwait(false);
                return;
            }

            using (var socket = await context.WebSockets.AcceptWebSocketAsync().ConfigureAwait(false)) {
                await WebSocketLoopAsync(socket).ConfigureAwait(false);
            }
        }
    }
}
