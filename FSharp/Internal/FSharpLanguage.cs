using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.FSharp.Compiler.AbstractIL.Internal;
using MirrorSharp.Internal.Abstraction;

namespace MirrorSharp.FSharp.Internal {
    [UsedImplicitly(ImplicitUseKindFlags.InstantiatedWithFixedConstructorSignature)]
    internal class FSharpLanguage : ILanguage {
        public const string Name = "F#";

        private readonly MirrorSharpFSharpOptions _options;

        static FSharpLanguage() {
            Library.Shim.FileSystem = CustomFileSystem.Instance;
        }

        public FSharpLanguage(MirrorSharpFSharpOptions options) {
            _options = options;
        }

        public ILanguageSession CreateSession(string text, OptimizationLevel? optimizationLevel) {
            return new FSharpSession(text, _options.AssemblyReferencePaths, optimizationLevel);
        }

        string ILanguage.Name => Name;
    }
}