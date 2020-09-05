using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MirrorSharp.Advanced;
using MirrorSharp.AspNetCore.Demo.Extensions;
using MirrorSharp.AspNetCore.Demo.Library;
using SharpLab.Server.Common.Internal;

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
                    o.MetadataReferences = o.MetadataReferences.AddRange(GetAllReferences());
                })
                .EnableFSharp()
            ));
        }

        private static IEnumerable<MetadataReference> GetAllReferences() {
            yield return ReferenceWithDocumentation(typeof(object).Assembly.Location);

            var referenceBasePath = Path.GetDirectoryName(typeof(object).Assembly.Location);
            yield return ReferenceWithDocumentation(Path.Join(referenceBasePath, "System.Runtime.dll"));

            var assembly = typeof(IScriptGlobals).Assembly;
            yield return MetadataReference.CreateFromFile(assembly.Location);
            foreach (var name in assembly.GetReferencedAssemblies()) {
                yield return ReferenceWithDocumentation(Assembly.Load(name).Location);
            }
        }

        private static MetadataReference ReferenceWithDocumentation(string path)
            => MetadataReference.CreateFromFile(path, documentation: XmlDocumentationResolver.GetProvider(path));
    }
}
