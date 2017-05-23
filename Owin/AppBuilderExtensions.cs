using MirrorSharp.Owin.Internal;
using Owin;
using JetBrains.Annotations;

namespace MirrorSharp.Owin {
    /// <summary>MirrorSharp-related extensions for the <see cref="IAppBuilder" />.</summary>
    [PublicAPI]
    public static class AppBuilderExtensions {
        /// <summary>Adds MirrorSharp middleware to the <see cref="IAppBuilder" />.</summary>
        /// <param name="app">The app builder.</param>
        /// <param name="options">The <see cref="MirrorSharpOptions" /> object used by the MirrorSharp middleware.</param>
        [NotNull]
        public static IAppBuilder UseMirrorSharp([NotNull] this IAppBuilder app, [CanBeNull] MirrorSharpOptions options = null) {
            Argument.NotNull(nameof(app), app);
            app.Use(typeof(Middleware), options ?? new MirrorSharpOptions());
            return app;
        }
    }
}
