using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using FSharp.Compiler.SourceCodeServices;
using Microsoft.FSharp.Core;
using System.IO;

namespace MirrorSharp.FSharp {
    /// <summary>MirrorSharp options for F#</summary>
    public class MirrorSharpFSharpOptions {
        internal MirrorSharpFSharpOptions() {
            var assemblyPaths = ImmutableArray.CreateBuilder<string>();

            var corelib = typeof(object).Assembly;
            assemblyPaths.Add(corelib.Location);

            // Initial version -- likely to need a lot of adjustment/alignment with other languages
            if (corelib.GetName().Name == "System.Private.CoreLib") {
                // .NET Core
                var basePath = Path.GetDirectoryName(corelib.Location);
                assemblyPaths.Add(Path.Combine(basePath, "mscorlib.dll"));
                assemblyPaths.Add(Path.Combine(basePath, "netstandard.dll"));
                assemblyPaths.Add(Path.Combine(basePath, "System.dll"));
                assemblyPaths.Add(Path.Combine(basePath, "System.Collections.dll"));
                assemblyPaths.Add(Path.Combine(basePath, "System.IO.dll"));
                assemblyPaths.Add(Path.Combine(basePath, "System.Net.Requests.dll"));
                assemblyPaths.Add(Path.Combine(basePath, "System.Net.WebClient.dll"));
                assemblyPaths.Add(Path.Combine(basePath, "System.Runtime.dll"));
                assemblyPaths.Add(Path.Combine(basePath, "System.Runtime.Extensions.dll"));
                assemblyPaths.Add(Path.Combine(basePath, "System.Runtime.Numerics.dll"));

            }
            assemblyPaths.Add(typeof(EntryPointAttribute).Assembly.Location);
            AssemblyReferencePaths = assemblyPaths.ToImmutable();
        }

        /// <summary>Specifies the list of assembly reference paths to be used.</summary>
        public ImmutableArray<string> AssemblyReferencePaths { get; set; }

        /// <summary>Corresponds to option <c>--debug</c> in <see cref="FSharpProjectOptions.OtherOptions"/>.</summary>
        public bool? Debug { get; set; }

        /// <summary>Corresponds to option <c>--optimize</c> in <see cref="FSharpProjectOptions.OtherOptions"/>.</summary>
        public bool? Optimize { get; set; }

        /// <summary>Corresponds to option <c>--target</c> in <see cref="FSharpProjectOptions.OtherOptions"/>.</summary>
        public string? Target { get; set; }

        /// <summary>Corresponds to option <c>--langversion</c> in <see cref="FSharpProjectOptions.OtherOptions"/>.</summary>
        public string? LangVersion { get; set;}
    }
}
