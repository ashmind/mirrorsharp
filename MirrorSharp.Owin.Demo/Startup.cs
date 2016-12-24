using Microsoft.Owin;
using MirrorSharp.Owin.Demo;
using Owin;

[assembly: OwinStartup(typeof(Startup), nameof(Startup.Configuration))]

namespace MirrorSharp.Owin.Demo {
    public class Startup {
        public void Configuration(IAppBuilder app) {
            app.UseDefaultFiles()
               .UseStaticFiles()
               .UseMirrorSharp(new MirrorSharpOptions { SelfDebugEnabled = true });
        }
    }
}
