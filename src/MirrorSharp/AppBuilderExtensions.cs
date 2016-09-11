using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Builder;
using MirrorSharp.Internal;

namespace MirrorSharp {
    public static class AppBuilderExtensions {
        public static void UseMirrorSharp([NotNull] this IApplicationBuilder app, [CanBeNull] MirrorSharpOptions options = null) {
            Argument.NotNull(nameof(app), app);
            app.UseMiddleware<Middleware>(options);
        }
    }
}
