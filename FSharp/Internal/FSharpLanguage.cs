using FSharp.Compiler.SourceCodeServices;
using MirrorSharp.Internal;
using MirrorSharp.Internal.Abstraction;

namespace MirrorSharp.FSharp.Internal {
    internal class FSharpLanguage : ILanguage {
        public const string Name = "F#";

        private readonly MirrorSharpFSharpOptions _options;

        static FSharpLanguage() {
            FileSystemAutoOpens.FileSystem = CustomFileSystem.Instance;
        }

        public FSharpLanguage(MirrorSharpFSharpOptions options) {
            _options = options;
        }

        public ILanguageSessionInternal CreateSession(string text, ILanguageSessionExtensions extensions) {
            return new FSharpSession(text, _options);
        }

        string ILanguage.Name => Name;
    }
}