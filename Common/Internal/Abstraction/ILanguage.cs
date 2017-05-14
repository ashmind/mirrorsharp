using JetBrains.Annotations;
using Microsoft.CodeAnalysis;

namespace MirrorSharp.Internal.Abstraction {
    internal interface ILanguage {
        [NotNull] string Name { get; }
        [NotNull] ILanguageSession CreateSession([NotNull] string text, OptimizationLevel? optimizationLevel);
    }
}
