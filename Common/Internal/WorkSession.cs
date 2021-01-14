using System.Collections.Generic;
using MirrorSharp.Advanced;
using MirrorSharp.Internal.Abstraction;
using MirrorSharp.Internal.Roslyn;

namespace MirrorSharp.Internal {
    internal class WorkSession : IWorkSession {
        private readonly ILanguageSessionExtensions _extensions;
        private ILanguage _language;
        private ILanguageSessionInternal? _languageSession;
        private string _lastText = "";

        public WorkSession(ILanguage language, IWorkSessionOptions options, ILanguageSessionExtensions extensions) {
            _language = Argument.NotNull(nameof(language), language);
            _extensions = extensions;

            SelfDebug = options.SelfDebugEnabled ? new SelfDebug() : null;
        }

        public void ChangeLanguage(ILanguage language) {
            Argument.NotNull(nameof(language), language);
            if (language == _language)
                return;
            _language = language;

            if (_languageSession != null) {
                _lastText = _languageSession.GetText();
                _languageSession.Dispose();
            }
            _languageSession = null;
        }

        private void Initialize() {
            _languageSession = Language.CreateSession(_lastText, _extensions);
        }

        public ILanguage Language => _language;
        string IWorkSession.LanguageName => Language.Name;
        
        public ILanguageSessionInternal LanguageSession {
            get {
                EnsureInitialized();
                return _languageSession!;
            }
        }

        public bool IsRoslyn => LanguageSession is RoslynSession;
        public RoslynSession Roslyn => (RoslynSession)LanguageSession;
        IRoslynSession IWorkSession.Roslyn => Roslyn;

        public string GetText() => LanguageSession.GetText();
        public void ReplaceText(string newText, int start = 0, int? length = null) => LanguageSession.ReplaceText(newText, start, length);
        public int CursorPosition { get; set; }

        public CurrentCompletion CurrentCompletion { get; } = new CurrentCompletion();

        public IDictionary<string, string> RawOptionsFromClient { get; } = new Dictionary<string, string>();
        public SelfDebug? SelfDebug { get; }
        public IDictionary<string, object> ExtensionData { get; } = new Dictionary<string, object>();

        private void EnsureInitialized() {
            if (_languageSession != null)
                return;
            Initialize();
        }

        public void Dispose() => _languageSession?.Dispose();
    }
}