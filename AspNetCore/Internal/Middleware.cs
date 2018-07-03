using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using MirrorSharp.Internal;

namespace MirrorSharp.AspNetCore.Internal {
    internal class Middleware : MiddlewareBase {
        private readonly RequestDelegate _next;

        public Middleware(MirrorSharpOptions options, RequestDelegate next) : base(options) {
            _next = next;
        }

        public Task InvokeAsync(HttpContext context) {
            if (!context.WebSockets.IsWebSocketRequest)
                return _next(context);

            return ProcessWebSocketRequest(context);
        }

        private async Task ProcessWebSocketRequest(HttpContext context) {
            var socket = await context.WebSockets.AcceptWebSocketAsync().ConfigureAwait(false);
            await WebSocketLoopAsync(socket, context.RequestAborted).ConfigureAwait(false);
        }
    }
}
