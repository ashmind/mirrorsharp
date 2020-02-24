using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MirrorSharp.AspNetCore.Demo.Extensions;

namespace MirrorSharp.AspNetCore.Demo {
    public class Startup {
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env) {
            if (env.IsDevelopment()) {
                app.UseDeveloperExceptionPage();
            }

            app.UseDefaultFiles()
               .UseStaticFiles();
            app.UseWebSockets();
            app.UseRouting();

            app.UseEndpoints(e => e.MapMirrorSharp(
                "/mirrorsharp",
                new MirrorSharpOptions {
                    SelfDebugEnabled = true,
                    IncludeExceptionDetails = true,
                    SetOptionsFromClient = new SetOptionsFromClientExtension()
                }
                .EnableFSharp()
            ));
        }
    }
}
