using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

#if QUICKINFO
namespace MirrorSharp.Internal.Reflection {
    internal static class VisualStudioAssemblyStubs {
        public static void Register() {
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
        }

        private static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args) {
            var name = new AssemblyName(args.Name);
            if (name.Name == "Microsoft.VisualStudio.Text.Data")
                return BuildTextDataAssembly(name);
            return null;
        }

        private static Assembly BuildTextDataAssembly(AssemblyName name) {
            var builder = AppDomain.CurrentDomain.DefineDynamicAssembly(name, AssemblyBuilderAccess.Run);
            return builder;
        }
    }
}
#endif