using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using MirrorSharp.Internal;

namespace MirrorSharp {
    public static class AppBuilderExtensions {
        public static void UseMirrorSharp(this IApplicationBuilder app) {
            app.UseMiddleware<Middleware>();
        }
    }
}
