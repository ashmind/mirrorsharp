using Microsoft.Owin;
using Owin;
using MirrorSharp.Owin.Demo;
using MirrorSharp.Owin.Demo.Extensions;

[assembly: OwinStartup(typeof(Startup), nameof(Startup.Configuration))]

namespace MirrorSharp.Owin.Demo {
    public class Startup {
        public void Configuration(IAppBuilder app) {
            app.UseDefaultFiles()
               .UseStaticFiles();

            app.UseMirrorSharp(new MirrorSharpOptions {
                SelfDebugEnabled = true,
                IncludeExceptionDetails = true,
                SetOptionsFromClient = new SetOptionsFromClientExtension()
            }.EnableFSharp());
        }
    }
}
