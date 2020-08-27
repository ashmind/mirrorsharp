using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using AspNetCore.Demo.Library;
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
                .SetupCSharp(o => {
                    o.AddMetadataReferencesFromFiles(GetAllReferencePaths().ToArray());
                    o.SetScriptMode(true, typeof(IScriptGlobals));
                })
                .EnableFSharp()
            ));
        }

        static IEnumerable<string> GetAllReferencePaths() {
            var assemblyPath = Path.GetDirectoryName(typeof(object).Assembly.Location)
                               ?? throw new InvalidOperationException("Could not find the assembly for object.");
            yield return Path.Combine(assemblyPath, "mscorlib.dll");
            yield return Path.Combine(assemblyPath, "System.dll");
            yield return Path.Combine(assemblyPath, "System.Core.dll");
            yield return Path.Combine(assemblyPath, "System.Runtime.dll");
            var assembly = typeof(IScriptGlobals).Assembly;
            yield return assembly.Location;
            foreach (var name in assembly.GetReferencedAssemblies()) {
                yield return Assembly.Load(name).Location;
            }
        }
    }
}
