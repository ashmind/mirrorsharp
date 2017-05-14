using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;

namespace MirrorSharp.Internal.Abstraction {
    internal interface ILanguage {
        [NotNull] string Name { get; }
        [NotNull] ParseOptions DefaultParseOptions { get; }
        [NotNull] CompilationOptions DefaultCompilationOptions { get; }

        [NotNull] ILanguageSession CreateSession([NotNull] string text, ParseOptions parseOptions, CompilationOptions compilationOptions, [CanBeNull] IReadOnlyCollection<MetadataReference> assemblyReferences);
    }
}
