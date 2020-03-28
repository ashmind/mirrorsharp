using System;
using Microsoft.Owin;
using Microsoft.Owin.StaticFiles;
using Microsoft.Owin.FileSystems;
using Owin;
using MirrorSharp.Owin.Demo;
using MirrorSharp.Owin.Demo.Extensions;
using System.IO;

[assembly: OwinStartup(typeof(Startup), nameof(Startup.Configuration))]

namespace MirrorSharp.Owin.Demo {
    public class Startup {
        private static readonly string MscorlibReferencePath = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86)
            + @"\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.6\mscorlib.dll";

        public void Configuration(IAppBuilder app) {
            app.UseDefaultFiles()
               .UseStaticFiles(new StaticFileOptions {
                   FileSystem = new PhysicalFileSystem("wwwroot")
               });

            app.MapMirrorSharp(
                "/mirrorsharp",

                new MirrorSharpOptions {
                    SelfDebugEnabled = true,
                    IncludeExceptionDetails = true
                }
                .SetupCSharp(c => {
                    c.MetadataReferences = c.MetadataReferences.Clear();
                    c.AddMetadataReferencesFromFiles(MscorlibReferencePath);
                })
                .EnableFSharp(),

                new MirrorSharpServices {
                    SetOptionsFromClient = new SetOptionsFromClientExtension()
                }
            );
        }
    }
}
