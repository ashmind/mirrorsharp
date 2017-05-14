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
        [NotNull] private readonly IDictionary<string, Func<ParseOptions, ParseOptions>> _parseOptionsChanges = new Dictionary<string, Func<ParseOptions, ParseOptions>>();
        [NotNull] private readonly IDictionary<string, Func<CompilationOptions, CompilationOptions>> _compilationOptionsChanges = new Dictionary<string, Func<CompilationOptions, CompilationOptions>>();
        [NotNull] private ILanguage _language;
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

        public void ChangeParseOptions([NotNull] string key, [NotNull] Func<ParseOptions, ParseOptions> change) {
            Argument.NotNull(nameof(key), key);
            Argument.NotNull(nameof(change), change);

            if (_parseOptionsChanges.TryGetValue(key, out var current) && current == change)
                return;
            _parseOptionsChanges[key] = change;
            if (_languageSession is RoslynSession roslyn && change(roslyn.Project.ParseOptions) == roslyn.Project.ParseOptions)
                return;
            Reset();
        }

        public void ChangeCompilationOptions([NotNull] string key, [NotNull] Func<CompilationOptions, CompilationOptions> change) {
            Argument.NotNull(nameof(key), key);
            Argument.NotNull(nameof(change), change);
            if (_compilationOptionsChanges.TryGetValue(key, out var current) && current == change)
                return;
            _compilationOptionsChanges[key] = change;
            if (_languageSession is RoslynSession roslyn && change(roslyn.Project.CompilationOptions) == roslyn.Project.CompilationOptions)
                return;
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
            var parseOptions = _options?.GetDefaultParseOptionsByLanguageName?.Invoke(Language.Name) ?? Language.DefaultParseOptions;
            foreach (var change in _parseOptionsChanges.Values) {
                parseOptions = change(parseOptions);
            }
            var compilationOptions = _options?.GetDefaultCompilationOptionsByLanguageName?.Invoke(Language.Name) ?? Language.DefaultCompilationOptions;
            foreach (var change in _compilationOptionsChanges.Values) {
                compilationOptions = change(compilationOptions);
            }
            var assemblyReferences = _options?.GetDefaultMetadataReferencesByLanguageName?.Invoke(Language.Name);
            _languageSession = Language.CreateSession(_lastText, parseOptions, compilationOptions, assemblyReferences);
        }

        public IWorkSessionOptions Options => _options;
        [NotNull] public ILanguage Language => _language;
        [NotNull]
        public ILanguageSession LanguageSession {
            get {
                EnsureInitialized();
                return _languageSession;
            }
        }

        [NotNull]   public RoslynSession Roslyn => (RoslynSession)LanguageSession;
        [CanBeNull] public RoslynSession RoslynOrNull => LanguageSession as RoslynSession;

        IRoslynSession IWorkSession.Roslyn => Roslyn;
        IRoslynSession IWorkSession.RoslynOrNull => RoslynOrNull;

        [NotNull] public string GetText() => LanguageSession.GetText();
        public void ReplaceText(string newText, int start = 0, [CanBeNull] int? length = null) => LanguageSession.ReplaceText(newText, start, length);
        public int CursorPosition { get; set; }

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