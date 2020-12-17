using System;
using System.ComponentModel;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using MirrorSharp.AspNetCore.Internal;
using MirrorSharp.Internal;

namespace MirrorSharp.AspNetCore {
    /// <summary>MirrorSharp setup extensions.</summary>
    public static class MirrorSharpStartupExtensions {
        /// <summary>Adds MirrorSharp middleware to the <see cref="IApplicationBuilder" />.</summary>
        /// <param name="app">The app builder.</param>
        /// <param name="options">The <see cref="MirrorSharpOptions" /> object used by the MirrorSharp middleware.</param>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("This method will be removed in the next major version. Use app.MapMirrorSharp() instead.")]
        public static IApplicationBuilder UseMirrorSharp(this IApplicationBuilder app, MirrorSharpOptions? options = null) {
            Argument.NotNull(nameof(app), app);
            app.UseMiddleware<Middleware>(options ?? new MirrorSharpOptions());
            return app;
        }

        /// <summary>Maps MirrorSharp middleware to a certain path in the <see cref="IApplicationBuilder" />.</summary>
        /// <param name="app">The app builder.</param>
        /// <param name="path">Relative path to be used by MirrorSharp server, e.g. '/mirrorsharp'.</param>
        /// <param name="options">The <see cref="MirrorSharpOptions" /> object used by the MirrorSharp middleware.</param>
        public static IApplicationBuilder MapMirrorSharp(this IApplicationBuilder app, PathString path, MirrorSharpOptions? options = null)
        {
            Argument.NotNull(nameof(app), app);
            Argument.NotNull(nameof(path), path.Value);

            return app.Map(path, a => a.UseMiddleware<Middleware>(options ?? new MirrorSharpOptions()));
        }

        /// <summary>Maps MirrorSharp middleware to a certain route in the <see cref="IEndpointRouteBuilder" />.</summary>
        /// <param name="endpoints">The endpoint route builder.</param>
        /// <param name="pattern">The route pattern to be used by MirrorSharp server, e.g. '/mirrorsharp'.</param>
        /// <param name="options">The <see cref="MirrorSharpOptions" /> object used by the MirrorSharp middleware.</param>
        public static IEndpointConventionBuilder MapMirrorSharp(this IEndpointRouteBuilder endpoints, string pattern, MirrorSharpOptions? options = null) {
            Argument.NotNull(nameof(endpoints), endpoints);
            Argument.NotNullOrEmpty(nameof(pattern), pattern);

            var pipeline = endpoints.CreateApplicationBuilder()
                .UseMiddleware<Middleware>(options ?? new MirrorSharpOptions())
                .Build();
            return endpoints.Map(pattern, pipeline);
        }
    }
}
