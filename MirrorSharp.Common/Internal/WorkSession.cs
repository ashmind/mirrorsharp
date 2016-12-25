using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;
using AshMind.Extensions;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Completion;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using MirrorSharp.Advanced;
using MirrorSharp.Internal.Languages;
using MirrorSharp.Internal.Reflection;

namespace MirrorSharp.Internal {
    public class WorkSession : IWorkSession {
        private static readonly TextChange[] NoTextChanges = new TextChange[0];

        [CanBeNull] private readonly IWorkSessionOptions _options;
        private CustomWorkspace _workspace;

        private SourceText _sourceText;
        private bool _documentOutOfDate;
        private Document _document;

        private readonly IDictionary<Type, object> _data = new Dictionary<Type, object>();
        private CompletionService _completionService;
        private ImmutableArray<DiagnosticAnalyzer> _analyzers;
        private ImmutableDictionary<string, ImmutableArray<CodeFixProvider>> _codeFixProviders;
        private ImmutableArray<ISignatureHelpProviderWrapper> _signatureHelpProviders;

        internal WorkSession([NotNull] ILanguage language, [CanBeNull] IWorkSessionOptions options = null) {
            Language = Argument.NotNull(nameof(language), language);
            _options = options;
            SelfDebug = (options?.SelfDebugEnabled ?? false) ? new SelfDebug() : null;
        }

        internal void ChangeLanguage([NotNull] ILanguage language) {
            Argument.NotNull(nameof(language), language);
            if (Language == language)
                return;

            Language = language;
            _workspace?.Dispose();
            _workspace = null;
        }

        private void Initialize() {
            var projectId = ProjectId.CreateNewId();
            var projectInfo = ProjectInfo.Create(
                projectId, VersionStamp.Create(), "_", "_", Language.Name,
                compilationOptions: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary),
                parseOptions: _options?.GetDefaultParseOptionsByLanguageName(Language.Name),
                metadataReferences: Language.DefaultAssemblyReferences,
                analyzerReferences: Language.DefaultAnalyzerReferences
            );
            var documentId = DocumentId.CreateNewId(projectId);
            _sourceText = SourceText.From("");

            _workspace = new CustomWorkspace(Language.HostServices);
            var solution = _workspace.CurrentSolution
                .AddProject(projectInfo)
                .AddDocument(documentId, "_", _sourceText);
            solution = _workspace.SetCurrentSolution(solution);
            _workspace.OpenDocument(documentId);
            _document = solution.GetDocument(documentId);
            _completionService = CompletionService.GetService(_document);
            if (CompletionService == null)
                throw new Exception("Failed to retrieve the completion service.");

            _analyzers = Language.DefaultAnalyzers;
            _codeFixProviders = Language.DefaultCodeFixProvidersIndexedByDiagnosticIds;
            _signatureHelpProviders = Language.DefaultSignatureHelpProviders;
        }

        internal ILanguage Language { get; private set; }

        public int CursorPosition { get; set; }

        public SourceText SourceText {
            get {
                EnsureInitialized();
                return _sourceText;
            }
            set {
                EnsureInitialized();
                if (value == _sourceText)
                    return;
                _sourceText = value;
                _documentOutOfDate = true;
            }
        }

        public Document Document {
            get {
                EnsureDocumentUpToDate();
                return _document;
            }
        }

        [NotNull]
        public CompletionService CompletionService {
            get {
                EnsureInitialized();
                return _completionService;
            }
        }

        [CanBeNull] public CompletionList CurrentCompletionList { get; set; }
        [NotNull] public IList<CodeAction> CurrentCodeActions { get; } = new List<CodeAction>();
        [CanBeNull] internal CurrentSignatureHelp? CurrentSignatureHelp { get; set; }

        public CustomWorkspace Workspace {
            get {
                EnsureDocumentUpToDate();
                return _workspace;
            }
        }
        public Project Project => Document.Project;

        public ImmutableArray<DiagnosticAnalyzer> Analyzers {
            get {
                EnsureInitialized();
                return _analyzers;
            }
        }

        public ImmutableDictionary<string, ImmutableArray<CodeFixProvider>> CodeFixProviders {
            get {
                EnsureInitialized();
                return _codeFixProviders;
            }
        }

        internal ImmutableArray<ISignatureHelpProviderWrapper> SignatureHelpProviders {
            get {
                EnsureInitialized();
                return _signatureHelpProviders;
            }
            private set { _signatureHelpProviders = value; }
        }

        [CanBeNull] public SelfDebug SelfDebug { get; }

        private void EnsureInitialized() {
            if (_workspace != null)
                return;
            Initialize();
        }

        private void EnsureDocumentUpToDate() {
            EnsureInitialized();
            if (!_documentOutOfDate)
                return;

            var document = _document.WithText(_sourceText);
            if (!_workspace.TryApplyChanges(document.Project.Solution))
                throw new Exception("Failed to apply changes to workspace.");
            _document = _workspace.CurrentSolution.GetDocument(document.Id);
            _documentOutOfDate = false;
        }

        public async Task<IReadOnlyList<TextChange>> UpdateFromWorkspaceAsync() {
            EnsureDocumentUpToDate();
            var project = _workspace.CurrentSolution.GetProject(Project.Id);
            if (project == Project)
                return NoTextChanges;

            var oldText = _sourceText;
            _document = project.GetDocument(_document.Id);
            _sourceText = await _document.GetTextAsync();
            return _sourceText.GetTextChanges(oldText);
        }

        public T Get<T>() => (T)_data.GetValueOrDefault(typeof(T));
        public void Set<T>(T value) => _data[typeof(T)] = value;

        public void Dispose() {
            _workspace.Dispose();
        }
    }
}



