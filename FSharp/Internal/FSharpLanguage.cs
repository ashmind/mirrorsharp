using System;
using System.Collections.Immutable;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.FSharp.Compiler.AbstractIL.Internal;
using Microsoft.FSharp.Core;
using MirrorSharp.Internal.Abstraction;

namespace MirrorSharp.FSharp.Internal {
    [UsedImplicitly(ImplicitUseKindFlags.InstantiatedWithFixedConstructorSignature)]
    internal class FSharpLanguage : ILanguage {
        public const string Name = "F#";
        
        private readonly ImmutableArray<string> _defaultAssemblyReferencePaths;

        static FSharpLanguage() {
            Library.Shim.FileSystem = new RestrictedFileSystem();
        }

        public FSharpLanguage(MirrorSharpFSharpOptions options) {
            var fsharpAssembly = typeof(FSharpOption<>).GetTypeInfo().Assembly;
            _defaultAssemblyReferencePaths = ImmutableArray.Create(
                // note: this currently does not work on .NET Core, which is why this project isn't netstandard
                typeof(object).GetTypeInfo().Assembly.Location,
                new Uri(fsharpAssembly.EscapedCodeBase).LocalPath
            );
        }

        public ILanguageSession CreateSession(string text, OptimizationLevel? optimizationLevel) {
            return new FSharpSession(text, _defaultAssemblyReferencePaths, optimizationLevel);
        }

        string ILanguage.Name => Name;
    }
}