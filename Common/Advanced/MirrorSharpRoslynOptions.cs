using System.Collections.Immutable;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;

namespace MirrorSharp.Advanced {
    [PublicAPI]
    public class MirrorSharpRoslynOptions<TParseOptions, TCompilationOptions>
        where TParseOptions : ParseOptions
        where TCompilationOptions : CompilationOptions
    {
        [CanBeNull] public TParseOptions ParseOptions { get; set; }
        [CanBeNull] public TCompilationOptions CompilationOptions { get; set; }
        [CanBeNull] public ImmutableList<MetadataReference> MetadataReferences { get; set; }
    }
}
