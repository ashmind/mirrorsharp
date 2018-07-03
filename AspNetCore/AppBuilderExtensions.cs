using JetBrains.Annotations;
using Microsoft.AspNetCore.Builder;
using MirrorSharp.AspNetCore.Internal;

namespace MirrorSharp.Owin {
    /// <summary>MirrorSharp-related extensions for the <see cref="IApplicationBuilder" />.</summary>
    [PublicAPI]
    public static class AppBuilderExtensions {
        /// <summary>Adds MirrorSharp middleware to the <see cref="IApplicationBuilder" />.</summary>
        /// <param name="app">The app builder.</param>
        /// <param name="options">The <see cref="MirrorSharpOptions" /> object used by the MirrorSharp middleware.</param>
        [NotNull]
        public static IApplicationBuilder UseMirrorSharp([NotNull] this IApplicationBuilder app, [CanBeNull] MirrorSharpOptions options = null) {
            Argument.NotNull(nameof(app), app);
            app.UseMiddleware<Middleware>(options ?? new MirrorSharpOptions());
            return app;
        }
    }
}
