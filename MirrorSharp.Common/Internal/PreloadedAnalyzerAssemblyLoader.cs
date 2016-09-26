using System.Reflection;
using Microsoft.CodeAnalysis;

namespace MirrorSharp.Internal {
    public class PreloadedAnalyzerAssemblyLoader : IAnalyzerAssemblyLoader {
        private readonly Assembly _assembly;

        public PreloadedAnalyzerAssemblyLoader(Assembly assembly) {
            _assembly = assembly;
        }

        public Assembly LoadFromPath(string fullPath) {
            return _assembly;
        }

        public void AddDependencyLocation(string fullPath) {
        }
    }
}
