using JetBrains.Annotations;
using Microsoft.CodeAnalysis;

namespace MirrorSharp.Internal.Abstraction {
    internal interface ILanguage {
        [NotNull] string Name { get; }
        [NotNull] ILanguageSessionInternal CreateSession([NotNull] string text);
    }
}
