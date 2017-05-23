using System;
using System.Collections.Immutable;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.FSharp.Core;

namespace MirrorSharp.FSharp {
    /// <summary>MirrorSharp options for F#</summary>
    [PublicAPI]
    public class MirrorSharpFSharpOptions {
        internal MirrorSharpFSharpOptions() {
            AssemblyReferencePaths = ImmutableArray.Create(
                // note: this currently does not work on .NET Core, which is why this project isn't netstandard
                typeof(object).GetTypeInfo().Assembly.Location,
                new Uri(typeof(FSharpOption<>).GetTypeInfo().Assembly.EscapedCodeBase).LocalPath
            );
        }

        /// <summary>Specifies the list of assembly reference paths to be used.</summary>
        public ImmutableArray<string> AssemblyReferencePaths { get; set; }
    }
}
