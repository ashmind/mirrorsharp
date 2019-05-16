using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using MirrorSharp.AspNetCore.Demo.Extensions;

namespace MirrorSharp.AspNetCore.Demo {
    public class Startup {
        public void ConfigureServices(IServiceCollection services) {
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env) {
            if (env.IsDevelopment()) {
                app.UseDeveloperExceptionPage();
            }

            app.UseDefaultFiles()
               .UseStaticFiles();

            app.UseWebSockets();
            app.UseMirrorSharp(
                new MirrorSharpOptions {
                    SelfDebugEnabled = true,
                    IncludeExceptionDetails = true,
                    SetOptionsFromClient = new SetOptionsFromClientExtension()
                }
                .EnableFSharp()
            );
        }
    }
}
