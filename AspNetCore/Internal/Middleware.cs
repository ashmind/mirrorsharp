using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using MirrorSharp.Internal;

namespace MirrorSharp.AspNetCore.Internal {
    internal class Middleware : MiddlewareBase {
        private readonly RequestDelegate _next;

        public Middleware(RequestDelegate next, MirrorSharpOptions options) : base(options) {
            _next = Argument.NotNull(nameof(next), next);
        }

        public Task InvokeAsync(HttpContext context) {
            if (!context.WebSockets.IsWebSocketRequest)
                return _next(context);

            return StartWebSocketLoopAsync(context);
        }

        public async Task StartWebSocketLoopAsync(HttpContext context) {
            var webSocket = await context.WebSockets.AcceptWebSocketAsync().ConfigureAwait(false);
            await WebSocketLoopAsync(webSocket, CancellationToken.None);
        }
    }
}