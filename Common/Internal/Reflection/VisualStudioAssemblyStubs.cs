using System;
using System.Reflection;

#if QUICKINFO
namespace MirrorSharp.Internal.Reflection {
    internal static class VisualStudioAssemblyStubs {
        public static void Register() {
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
        }

        private static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args) {
            var name = new AssemblyName(args.Name);
            if (name.Name.StartsWith("Microsoft.VisualStudio."))
                return Assembly.LoadFrom($@"d:\Development\VS 2017\MirrorSharp\Stubs\{name.Name}\bin\Debug\net46\{name.Name}.dll");

            return null;
        }
    }
}
#endif