using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MirrorSharp.Advanced;
using MirrorSharp.AspNetCore.Demo.Extensions;

namespace MirrorSharp.AspNetCore.Demo {
    public class Startup {
        public void ConfigureServices(IServiceCollection services) {
            services.AddSingleton<ISetOptionsFromClientExtension, SetOptionsFromClientExtension>();
        }

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
                    IncludeExceptionDetails = true
                }
                .EnableFSharp()
            ));
        }
    }
}
