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
            Library.Shim.FileSystem = CustomFileSystem.Instance;
        }

        public FSharpLanguage(MirrorSharpFSharpOptions options) {
            _defaultAssemblyReferencePaths = options.AssemblyReferencePaths ?? ImmutableArray.Create(
                // note: this currently does not work on .NET Core, which is why this project isn't netstandard
                typeof(object).GetTypeInfo().Assembly.Location,
                new Uri(typeof(FSharpOption<>).GetTypeInfo().Assembly.EscapedCodeBase).LocalPath
            );
        }

        public ILanguageSession CreateSession(string text, OptimizationLevel? optimizationLevel) {
            return new FSharpSession(text, _defaultAssemblyReferencePaths, optimizationLevel);
        }

        string ILanguage.Name => Name;
    }
}