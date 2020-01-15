using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using MirrorSharp.Internal;

namespace MirrorSharp.AspNetCore.Internal {
    internal class Middleware : MiddlewareBase {
        private readonly RequestDelegate _next;
        private readonly MirrorSharpOptions _options;

        public Middleware(RequestDelegate next, MirrorSharpOptions options) : base(options) {
            _next = Argument.NotNull(nameof(next), next);
            _options = options;
        }

        public Task InvokeAsync(HttpContext context) {

            if (!context.WebSockets.IsWebSocketRequest) {
                return _next(context);
            }

            if (string.IsNullOrWhiteSpace(_options.WebSocketUrl)) {
                if (!context.Request.Path.Value.EndsWith("/mirrorsharp", StringComparison.OrdinalIgnoreCase)) {
                    return _next(context);
                }
            } else {
                if (!context.Request.Path.Value.Equals(_options.WebSocketUrl.Trim(), StringComparison.OrdinalIgnoreCase)) {
                    return _next(context);
                }
            }   
            
            return StartWebSocketLoopAsync(context);
        }

        public async Task StartWebSocketLoopAsync(HttpContext context) {
            var webSocket = await context.WebSockets.AcceptWebSocketAsync().ConfigureAwait(false);
            await WebSocketLoopAsync(webSocket, CancellationToken.None);
        }
    }
}
