using System;
using System.Reflection;
using FSharp.Compiler.IO;

namespace MirrorSharp.FSharp.Internal {
    internal class CustomAssemblyLoader : IAssemblyLoader {
        public Assembly AssemblyLoadFrom(string assemboy) {
            throw new NotSupportedException();
        }

        public Assembly AssemblyLoad(AssemblyName assemblyName) {
            return Assembly.Load(assemblyName);
        }
    }
}