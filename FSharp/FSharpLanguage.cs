using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using MirrorSharp.Internal.Abstraction;

namespace MirrorSharp.FSharp {
    internal class FSharpLanguage : ILanguage {
        public const string Name = "F#";

        public ParseOptions DefaultParseOptions => null;
        public CompilationOptions DefaultCompilationOptions => null;
        public ImmutableList<MetadataReference> DefaultAssemblyReferences => null;

        public ILanguageSession CreateSession(string text, ParseOptions parseOptions, CompilationOptions compilationOptions, ImmutableList<MetadataReference> metadataReferences) {
            return new FSharpSession(text);
        }

        string ILanguage.Name => Name;
    }
}