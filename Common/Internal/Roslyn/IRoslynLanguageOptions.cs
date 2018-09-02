using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace MirrorSharp.Internal.Roslyn {
    internal interface IRoslynLanguageOptions {
        ParseOptions ParseOptions { get; }
        CompilationOptions CompilationOptions { get; }
        ImmutableList<MetadataReference> MetadataReferences { get; }

        bool IsScript { get; }
        Type HostObjectType { get; }
    }
}
