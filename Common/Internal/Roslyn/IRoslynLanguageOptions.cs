using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace MirrorSharp.Internal.Roslyn {
    internal interface IRoslynLanguageOptions {
        ParseOptions ParseOptions { get; }
        CompilationOptions CompilationOptions { get; }
        ImmutableList<MetadataReference> MetadataReferences { get; }
        ImmutableList<AnalyzerReference> AnalyzerReferences { get; }

        bool IsScript { get; }
        Type? HostObjectType { get; }
    }
}
