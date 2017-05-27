using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using MirrorSharp.Advanced;
using MirrorSharp.Internal.Abstraction;
using MirrorSharp.Internal.Roslyn;

namespace MirrorSharp.Internal {
    internal class WorkSession : IWorkSession {
        [CanBeNull] private readonly IWorkSessionOptions _options;
        [NotNull] private readonly IDictionary<string, Func<CompilationOptions, CompilationOptions>> _compilationOptionsChanges = new Dictionary<string, Func<CompilationOptions, CompilationOptions>>();
        [NotNull] private ILanguage _language;
        [CanBeNull] private OptimizationLevel? _optimizationLevel;
        private ILanguageSession _languageSession;
        private string _lastText = "";

        public WorkSession([NotNull] ILanguage language, [CanBeNull] IWorkSessionOptions options = null) {
            _language = Argument.NotNull(nameof(language), language);
            _options = options;

            SelfDebug = (options?.SelfDebugEnabled ?? false) ? new SelfDebug() : null;
        }

        public void ChangeLanguage([NotNull] ILanguage language) {
            Argument.NotNull(nameof(language), language);
            if (language == _language)
                return;
            _language = language;
            Reset();
        }

        public void ChangeOptimizationLevel([CanBeNull] OptimizationLevel? optimizationLevel) {
            if (optimizationLevel == _optimizationLevel)
                return;
            _optimizationLevel = optimizationLevel;
            Reset();
        }

        private void Reset() {
            if (_languageSession != null) {
                _lastText = _languageSession.GetText();
                _languageSession.Dispose();
            }
            _languageSession = null;
        }

        private void Initialize() {
            _languageSession = Language.CreateSession(_lastText, OptimizationLevel);
        }

        public IWorkSessionOptions Options => _options;
        public OptimizationLevel? OptimizationLevel => _optimizationLevel;
        [NotNull] public ILanguage Language => _language;
        string IWorkSession.LanguageName => Language.Name;
        [NotNull]
        public ILanguageSession LanguageSession {
            get {
                EnsureInitialized();
                return _languageSession;
            }
        }

        public bool IsRoslyn => LanguageSession is RoslynSession;
        [NotNull]   public RoslynSession Roslyn => (RoslynSession)LanguageSession;
        IRoslynSession IWorkSession.Roslyn => Roslyn;

        [NotNull] public string GetText() => LanguageSession.GetText();
        public void ReplaceText(string newText, int start = 0, [CanBeNull] int? length = null) => LanguageSession.ReplaceText(newText, start, length);
        public int CursorPosition { get; set; }

        [NotNull] public CurrentCompletion CurrentCompletion { get; } = new CurrentCompletion();

        [NotNull] public IDictionary<string, string> RawOptionsFromClient { get; } = new Dictionary<string, string>();
        [CanBeNull] public SelfDebug SelfDebug { get; }
        public IDictionary<string, object> ExtensionData { get; } = new Dictionary<string, object>();

        private void EnsureInitialized() {
            if (_languageSession != null)
                return;
            Initialize();
        }

        public void Dispose() => _languageSession?.Dispose();
    }
}