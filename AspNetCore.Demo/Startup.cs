using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MirrorSharp.Advanced;
using MirrorSharp.AspNetCore.Demo.Extensions;
using MirrorSharp.AspNetCore.Demo.Library;

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
                    o.MetadataReferences = GetAllReferences().ToImmutableList();
                })
                .EnableFSharp()
            ));
        }

        private static IEnumerable<MetadataReference> GetAllReferences() {
            yield return ReferenceAssembly("System.Runtime");
            yield return ReferenceAssembly("System.Collections");
            var assembly = typeof(IScriptGlobals).Assembly;
            yield return MetadataReference.CreateFromFile(assembly.Location);
            foreach (var reference in assembly.GetReferencedAssemblies()) {
                yield return ReferenceAssembly(reference.Name!);
            }
        }

        private static MetadataReference ReferenceAssembly(string name) {
            var rootPath = Path.Combine(
                Path.GetDirectoryName(new Uri(typeof(Startup).Assembly.EscapedCodeBase).LocalPath)!,
                "ref-assemblies"
            );
            var assemblyPath = Path.Combine(rootPath, name + ".dll");
            var documentationPath = Path.Combine(rootPath, name + ".xml");

            return MetadataReference.CreateFromFile(
                assemblyPath, documentation: XmlDocumentationProvider.CreateFromFile(documentationPath)
            );
        }
    }
}
