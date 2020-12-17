namespace MirrorSharp.Internal.Abstraction {
    internal interface ILanguage {
        string Name { get; }
        ILanguageSessionInternal CreateSession(string text, ILanguageSessionExtensions services);
    }
}
