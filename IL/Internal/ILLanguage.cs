using MirrorSharp.Internal;
using MirrorSharp.Internal.Abstraction;

namespace MirrorSharp.IL.Internal {
    // ReSharper disable once InconsistentNaming
    internal class ILLanguage : ILanguage {
        public static string Name = "IL";

        string ILanguage.Name => Name;

        public ILanguageSessionInternal CreateSession(string text, ILanguageSessionExtensions services)
            => new ILSession(text);
    }
}
