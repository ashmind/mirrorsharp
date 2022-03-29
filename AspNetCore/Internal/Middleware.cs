using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using MirrorSharp.Advanced;
using MirrorSharp.Advanced.EarlyAccess;
using MirrorSharp.Internal;

namespace MirrorSharp.AspNetCore.Internal {
    internal class Middleware : MiddlewareBase {
        private readonly RequestDelegate _next;
        private readonly IHostApplicationLifetime _applicationLifetime;
        private readonly MirrorSharpOptions _options;

        public Middleware(
            RequestDelegate next,
            IHostApplicationLifetime applicationLifetime,
            MirrorSharpOptions options,
            ISetOptionsFromClientExtension? setOptionsFromClient = null,
            ISlowUpdateExtension? slowUpdate = null,
            IRoslynSourceTextGuard? roslynSourceTextGuard = null,
            IRoslynCompilationGuard? roslynCompilationGuard = null,
            IConnectionSendViewer? connectionSendViewer = null,
            IExceptionLogger? exceptionLogger = null
        ) : base(options, new ImmutableExtensionServices(
            #pragma warning disable CS0618 // Type or member is obsolete
            setOptionsFromClient ?? options.SetOptionsFromClient,
            slowUpdate ?? options.SlowUpdate,
            roslynSourceTextGuard,
            roslynCompilationGuard,
            connectionSendViewer,
            exceptionLogger ?? options.ExceptionLogger
            #pragma warning restore CS0618
        )) {
            _next = Argument.NotNull(nameof(next), next);
            _applicationLifetime = Argument.NotNull(nameof(applicationLifetime), applicationLifetime);
            _options = Argument.NotNull(nameof(options), options);
        }

        public Task InvokeAsync(HttpContext context) {
            if (!context.WebSockets.IsWebSocketRequest) {
                if (context.Request.Path == "/status")
                    return SlowTestAndReportStatusAsync(context);

                return _next(context);
            }

            return StartWebSocketLoopAsync(context);
        }

        private async Task SlowTestAndReportStatusAsync(HttpContext context) {
            try {
                await SlowTestStatusAsync(context.RequestAborted).ConfigureAwait(false);
            }
            catch (Exception ex) {
                // TODO: Consolidate constant message with the other usage
                var message = (_options?.IncludeExceptionDetails ?? false)
                    ? ex.ToString()
                    : "A server error has occurred.";
                context.Response.StatusCode = 500;
                await context.Response.WriteAsync(message).ConfigureAwait(false);
                return;
            }

            context.Response.StatusCode = 200;
            await context.Response.WriteAsync("OK");
        }

        private async Task StartWebSocketLoopAsync(HttpContext context) {
            using var cancellationSource = new CancellationTokenSource();
            using var requestRegistration = context.RequestAborted.Register(() => cancellationSource.Cancel());
            using var applicationStoppingRegistration = _applicationLifetime.ApplicationStopping.Register(() => cancellationSource.Cancel());

            var webSocket = await context.WebSockets.AcceptWebSocketAsync().ConfigureAwait(false);
            await WebSocketLoopAsync(webSocket, cancellationSource.Token).ConfigureAwait(false);
        }
    }
}