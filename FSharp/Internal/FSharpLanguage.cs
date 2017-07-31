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

        public ILanguageSessionInternal CreateSession(string text) {
            return new FSharpSession(text, _options);
        }

        string ILanguage.Name => Name;
    }
}