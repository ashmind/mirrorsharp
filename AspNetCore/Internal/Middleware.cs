using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using MirrorSharp.Advanced;
using MirrorSharp.Advanced.EarlyAccess;
using MirrorSharp.Internal;

namespace MirrorSharp.AspNetCore.Internal {
    internal class Middleware : MiddlewareBase {
        private readonly RequestDelegate _next;
        private readonly MirrorSharpOptions _options;

        public Middleware(
            RequestDelegate next,
            MirrorSharpOptions options,
            ISetOptionsFromClientExtension? setOptionsFromClient = null,
            ISlowUpdateExtension? slowUpdate = null,
            IRoslynGuard? roslynGuard = null,
            IExceptionLogger? exceptionLogger = null
        ) : base(options, new ImmutableExtensionServices(
            #pragma warning disable CS0618 // Type or member is obsolete
            setOptionsFromClient ?? options.SetOptionsFromClient,
            slowUpdate ?? options.SlowUpdate,
            roslynGuard,
            exceptionLogger ?? options.ExceptionLogger
            #pragma warning restore CS0618
        )) {
            _next = Argument.NotNull(nameof(next), next);
            _options = Argument.NotNull(nameof(options), options);
        }

        public Task InvokeAsync(HttpContext context) {

            if (!context.WebSockets.IsWebSocketRequest) {
                return _next(context);
            }

            if (string.IsNullOrWhiteSpace(_options?.WebSocketUrl)) {
                if (!context.Request.Path.Value.EndsWith("/mirrorsharp", StringComparison.OrdinalIgnoreCase)) {
                    return _next(context);
                }
            } else {
                if (!context.Request.Path.Value.Equals(_options?.WebSocketUrl?.Trim(), StringComparison.OrdinalIgnoreCase)) {
                    return _next(context);
                }
            }   
            
            return StartWebSocketLoopAsync(context);
        }

        private async Task StartWebSocketLoopAsync(HttpContext context) {
            var webSocket = await context.WebSockets.AcceptWebSocketAsync().ConfigureAwait(false);
            await WebSocketLoopAsync(webSocket, CancellationToken.None);
        }
    }
}
