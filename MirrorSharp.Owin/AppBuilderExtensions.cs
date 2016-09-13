using MirrorSharp.Owin.Internal;
using Owin;
using JetBrains.Annotations;

namespace MirrorSharp.Owin {
    public static class AppBuilderExtensions {
        public static void UseMirrorSharp([NotNull] this IAppBuilder app, [CanBeNull] MirrorSharpOptions options = null) {
            Argument.NotNull(nameof(app), app);
            app.Use(typeof(Middleware), options);
        }
    }
}
