using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;

namespace MirrorSharp {
    public class MirrorSharpRoslynOptions<TParseOptions, TCompilationOptions>
        where TParseOptions : ParseOptions
        where TCompilationOptions : CompilationOptions
    {
        [CanBeNull] public TParseOptions ParseOptions { get; set; }
        [CanBeNull] public TCompilationOptions CompilationOptions { get; set; }
        [CanBeNull] public ImmutableList<MetadataReference> MetadataReferences { get; set; }
    }
}
