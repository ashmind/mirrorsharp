using JetBrains.Annotations;
using Microsoft.AspNetCore.Builder;
using MirrorSharp.AspNetCore.Internal;

namespace MirrorSharp.AspNetCore {
    public static class ApplicationBuilderExtensions {
        public static void UseMirrorSharp([NotNull] this IApplicationBuilder app, [CanBeNull] MirrorSharpOptions options = null) {
            Argument.NotNull(nameof(app), app);
            app.UseMiddleware<Middleware>(options);
        }
    }
}
