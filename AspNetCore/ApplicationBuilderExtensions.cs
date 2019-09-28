using Microsoft.AspNetCore.Builder;
using MirrorSharp.AspNetCore.Internal;

namespace MirrorSharp.AspNetCore {
    /// <summary>MirrorSharp-related extensions for the <see cref="IApplicationBuilder" />.</summary>
    public static class ApplicationBuilderExtensions {
        /// <summary>Adds MirrorSharp middleware to the <see cref="IApplicationBuilder" />.</summary>
        /// <param name="app">The app builder.</param>
        /// <param name="options">The <see cref="MirrorSharpOptions" /> object used by the MirrorSharp middleware.</param>
        public static IApplicationBuilder UseMirrorSharp(this IApplicationBuilder app, MirrorSharpOptions? options = null) {
            Argument.NotNull(nameof(app), app);
            app.UseMiddleware<Middleware>(options ?? new MirrorSharpOptions());
            return app;
        }
    }
}
