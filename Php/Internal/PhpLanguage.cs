using MirrorSharp.Internal;
using MirrorSharp.Internal.Abstraction;

namespace MirrorSharp.Php.Internal {
    internal class PhpLanguage : ILanguage {
        public const string Name = "PHP";

        private readonly MirrorSharpPhpOptions _options;

        public PhpLanguage(MirrorSharpPhpOptions options) {
            _options = options;
        }

        public ILanguageSessionInternal CreateSession(string text, ILanguageSessionExtensions extensions) {
            return new PhpSession(text, _options);
        }

        string ILanguage.Name => Name;
    }
}
