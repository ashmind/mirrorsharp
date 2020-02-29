using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using MirrorSharp.Advanced;
using MirrorSharp.Internal;

namespace MirrorSharp.AspNetCore.Internal {
    internal class Middleware : MiddlewareBase {
        private readonly RequestDelegate _next;

        public Middleware(
            RequestDelegate next,
            MirrorSharpOptions options,
            ISetOptionsFromClientExtension? setOptionsFromClient = null,
            ISlowUpdateExtension? slowUpdate = null,
            IExceptionLogger? exceptionLogger = null
        ) : base(options, new ImmutableExtensionServices(
            #pragma warning disable CS0618 // Type or member is obsolete
            setOptionsFromClient ?? options.SetOptionsFromClient,
            slowUpdate ?? options.SlowUpdate,
            exceptionLogger ?? options.ExceptionLogger
            #pragma warning restore CS0618
        )) {
            _next = Argument.NotNull(nameof(next), next);
        }

        public Task InvokeAsync(HttpContext context) {
            if (!context.WebSockets.IsWebSocketRequest)
                return _next(context);

            return StartWebSocketLoopAsync(context);
        }

        private async Task StartWebSocketLoopAsync(HttpContext context) {
            var webSocket = await context.WebSockets.AcceptWebSocketAsync().ConfigureAwait(false);
            await WebSocketLoopAsync(webSocket, CancellationToken.None);
        }
    }
}