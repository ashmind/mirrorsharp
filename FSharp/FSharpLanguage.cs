using System;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.FSharp.Compiler.AbstractIL.Internal;
using Microsoft.FSharp.Core;
using MirrorSharp.Internal.Abstraction;

namespace MirrorSharp.FSharp {
    internal class FSharpLanguage : ILanguage {
        public const string Name = "F#";

        public ParseOptions DefaultParseOptions => null;
        public CompilationOptions DefaultCompilationOptions => null;
        public ImmutableList<MetadataReference> DefaultAssemblyReferences { get; }

        static FSharpLanguage() {
            Library.Shim.FileSystem = new RestrictedFileSystem();
        }

        public FSharpLanguage() {
            var fsharpAssembly = typeof(FSharpOption<>).GetTypeInfo().Assembly;
            DefaultAssemblyReferences = ImmutableList.Create<MetadataReference>(
                // note: this currently does not work on .NET Core, which is why this project isn't netstandard
                MetadataReference.CreateFromFile(typeof(object).GetTypeInfo().Assembly.Location),
                MetadataReference.CreateFromFile(new Uri(fsharpAssembly.EscapedCodeBase).LocalPath)
            );
        }

        public ILanguageSession CreateSession(string text, ParseOptions parseOptions, CompilationOptions compilationOptions, ImmutableList<MetadataReference> metadataReferences) {
            return new FSharpSession(text, metadataReferences);
        }

        string ILanguage.Name => Name;
    }
}