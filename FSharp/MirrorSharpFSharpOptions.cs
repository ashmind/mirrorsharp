using System.Collections.Immutable;
using JetBrains.Annotations;

namespace MirrorSharp.FSharp {
    [PublicAPI]
    public class MirrorSharpFSharpOptions {
        [CanBeNull] public ImmutableArray<string>? AssemblyReferencePaths { get; set; }
    }
}
