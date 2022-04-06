using System;
#if !NETCOREAPP
using System.Buffers;
#endif
using System.Reflection;
#if NETCOREAPP
using System.Runtime.Loader;
#endif

namespace MirrorSharp.Internal.Roslyn {
    internal class RoslynInternalsLoader {
        public Assembly LoadInternalsAssemblySlow(RoslynInternalsLoadStrategy strategy) {
            var roslynVersion = RoslynAssemblies.MicrosoftCodeAnalysis.GetName().Version!;
            var assembly = LoadInternalsAssemblySlow(roslynVersion);
            if (strategy == RoslynInternalsLoadStrategy.TryMatchVersionThenLatest) {
                try {
                    _ = assembly.DefinedTypes;
                }
                catch (ReflectionTypeLoadException) {
                    return LoadInternalsAssemblySlow(new Version(999, 0));
                }
            }
            return assembly;
        }

        private Assembly LoadInternalsAssemblySlow(Version roslynVersion) {
            var assemblyName = roslynVersion switch {
                { Major: > 4 } => "MirrorSharp.Internal.Roslyn43.dll",
                { Major: 4, Minor: >= 3 } => "MirrorSharp.Internal.Roslyn43.dll",
                { Major: 4, Minor: 2 } => "MirrorSharp.Internal.Roslyn42.dll",
                { Major: 4, Minor: 1 } => "MirrorSharp.Internal.Roslyn41.dll",
                { Major: 4 } => "MirrorSharp.Internal.Roslyn36.dll",
                { Major: 3, Minor: >= 6 } => "MirrorSharp.Internal.Roslyn36.dll",
                { Major: 3, Minor: >= 3 } => "MirrorSharp.Internal.Roslyn33.dll",
                _ => throw new NotSupportedException()
            };

            using var assemblyStream = typeof(RoslynInternals).Assembly.GetManifestResourceStream(assemblyName)!;
            #if NETCOREAPP
            return AssemblyLoadContext.Default.LoadFromStream(assemblyStream);
            #else
            byte[]? assemblyBytes = null;
            try {
                assemblyBytes = ArrayPool<byte>.Shared.Rent((int)assemblyStream.Length);
                assemblyStream.Read(assemblyBytes, 0, assemblyBytes.Length);
                return Assembly.Load(assemblyBytes);
            }
            finally {
                if (assemblyBytes != null)
                    ArrayPool<byte>.Shared.Return(assemblyBytes);
            }
            #endif
        }
    }
}
