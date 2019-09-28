using MirrorSharp.Owin.Internal;
using Owin;

namespace MirrorSharp.Owin {
    /// <summary>MirrorSharp-related extensions for the <see cref="IAppBuilder" />.</summary>
    public static class AppBuilderExtensions {
        /// <summary>Adds MirrorSharp middleware to the <see cref="IAppBuilder" />.</summary>
        /// <param name="app">The app builder.</param>
        /// <param name="options">The <see cref="MirrorSharpOptions" /> object used by the MirrorSharp middleware.</param>
        public static IAppBuilder UseMirrorSharp(this IAppBuilder app, MirrorSharpOptions? options = null) {
            Argument.NotNull(nameof(app), app);
            app.Use(typeof(Middleware), options ?? new MirrorSharpOptions());
            return app;
        }
    }
}
