using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Completion;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Host.Mef;
using Microsoft.CodeAnalysis.Options;
using Microsoft.CodeAnalysis.QuickInfo;
using Microsoft.CodeAnalysis.Text;
using MirrorSharp.Advanced;
using MirrorSharp.Internal.Abstraction;
using MirrorSharp.Internal.Reflection;

namespace MirrorSharp.Internal.Roslyn {
    internal class RoslynSession : ILanguageSessionInternal, IRoslynSession {
        private static AnalyzerOptions EmptyAnalyzerOptions = new AnalyzerOptions(ImmutableArray<AdditionalText>.Empty);
        private static OptionSet EmptyOptionSet = RoslynReflection.NewWorkspaceOptionSet();

        private static readonly TextChange[] NoTextChanges = new TextChange[0];

        private readonly TextChange[] _oneTextChange = new TextChange[1];
        private readonly CustomWorkspace _workspace;

        private bool _documentOutOfDate;
        private Document _document;
        private SourceText _sourceText;

        private Solution _lastWorkspaceAnalyzerOptionsSolution;
        private AnalyzerOptions _workspaceAnalyzerOptions;

        private readonly QuickInfoService _quickInfoService;
        private readonly CompletionService _completionService;

        public RoslynSession(
            SourceText sourceText,
            ProjectInfo projectInfo,
            MefHostServices hostServices,
            ImmutableArray<DiagnosticAnalyzer> analyzers,
            ImmutableDictionary<string, ImmutableArray<CodeFixProvider>> codeFixProviders,
            ImmutableArray<ISignatureHelpProviderWrapper> signatureHelpProviders
        ) {
            _workspace = new CustomWorkspace(hostServices);
            _sourceText = sourceText;
            _document = CreateProjectAndOpenNewDocument(_workspace, projectInfo, sourceText);
            _quickInfoService = QuickInfoService.GetService(_document);
            _completionService = CompletionService.GetService(_document);

            Analyzers = analyzers;
            SignatureHelpProviders = signatureHelpProviders;
            CodeFixProviders = codeFixProviders;
        }

        private Document CreateProjectAndOpenNewDocument(Workspace workspace, ProjectInfo projectInfo, SourceText sourceText) {
            var documentId = DocumentId.CreateNewId(projectInfo.Id);
            var solution = _workspace.CurrentSolution
                .AddProject(projectInfo)
                .AddDocument(documentId, "_", sourceText);
            solution = _workspace.SetCurrentSolution(solution);
            workspace.OpenDocument(documentId);
            return solution.GetDocument(documentId);
        }

        public string GetText() => SourceText.ToString();
        public void ReplaceText(string newText, int start = 0, int? length = null) {
            var finalLength = length ?? SourceText.Length - start;
            _oneTextChange[0] = new TextChange(new TextSpan(start, finalLength), newText);
            SourceText = SourceText.WithChanges(_oneTextChange);
        }

        public async Task<ImmutableArray<Diagnostic>> GetDiagnosticsAsync(CancellationToken cancellationToken) {
            var compilation = await Project.GetCompilationAsync(cancellationToken).ConfigureAwait(false);
            var solution = Project.Solution;
            if (_lastWorkspaceAnalyzerOptionsSolution != solution) {
                _workspaceAnalyzerOptions = RoslynReflection.NewWorkspaceAnalyzerOptions(EmptyAnalyzerOptions, EmptyOptionSet, solution);
                _lastWorkspaceAnalyzerOptionsSolution = solution;
            }

            return await compilation.WithAnalyzers(Analyzers, _workspaceAnalyzerOptions, cancellationToken)
                .GetAllDiagnosticsAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        public Task<QuickInfoItem> GetInfoAsync(int cursorPosition, CancellationToken cancellationToken) {
            return _quickInfoService.GetQuickInfoAsync(Document, cursorPosition, cancellationToken);
        }

        public bool ShouldTriggerCompletion(int cursorPosition, CompletionTrigger trigger) {
            return _completionService.ShouldTriggerCompletion(SourceText, cursorPosition, trigger);
        }

        public Task<CompletionList> GetCompletionsAsync(int cursorPosition, CompletionTrigger trigger, CancellationToken cancellationToken) {
            return _completionService.GetCompletionsAsync(Document, cursorPosition, trigger, cancellationToken: cancellationToken);
        }

        public Task<CompletionChange> GetCompletionChangeAsync(TextSpan completionSpan, CompletionItem item, CancellationToken cancellationToken) {
            return _completionService.GetChangeAsync(Document, item, cancellationToken: cancellationToken);
        }

        [NotNull] public IList<CodeAction> CurrentCodeActions { get; } = new List<CodeAction>();

        [NotNull]
        public CustomWorkspace Workspace {
            get {
                EnsureDocumentUpToDate();
                return _workspace;
            }
        }

        public Project Project {
            get => Document.Project;
            set {
                Argument.NotNull(nameof(value), value);
                if (!value.ContainsDocument(_document.Id))
                    throw new ArgumentException($"Project must include session source document (id: '{_document.Id}').", nameof(value));
                if (_documentOutOfDate)
                    throw new InvalidOperationException("Source document has changed since getting Project; Project cannot be set.");
                ApplySolutionChange(value.Solution);
            }
        }

        [NotNull]
        public Document Document {
            get {
                EnsureDocumentUpToDate();
                return _document;
            }
        }

        public SourceText SourceText {
            get => _sourceText;
            set {
                if (value == _sourceText)
                    return;
                _sourceText = value;
                _documentOutOfDate = true;
            }
        }

        [CanBeNull] public CurrentSignatureHelp? CurrentSignatureHelp { get; set; }

        public ImmutableArray<DiagnosticAnalyzer> Analyzers { get; }
        public ImmutableDictionary<string, ImmutableArray<CodeFixProvider>> CodeFixProviders { get; }
        public ImmutableArray<ISignatureHelpProviderWrapper> SignatureHelpProviders { get; }

        public void SetScriptMode(bool isScript = true, Type hostObjectType = null) {
            RoslynScriptHelper.Validate(isScript, hostObjectType);
            var sourceKind = RoslynScriptHelper.GetSourceKind(isScript);

            var project = Project;
            var document = Document;

            var newProjectInfo = ProjectInfo.Create(
                project.Id,
                project.Version,
                project.Name,
                project.AssemblyName,
                project.Language,
                parseOptions: project.ParseOptions.WithKind(sourceKind),
                compilationOptions: project.CompilationOptions,
                documents: new[] {
                    DocumentInfo.Create(document.Id, document.Name, sourceCodeKind: sourceKind)
                },
                metadataReferences: project.MetadataReferences,
                analyzerReferences: project.AnalyzerReferences,
                isSubmission: isScript,
                hostObjectType: hostObjectType
            );
            var newSolution = project.Solution
                .RemoveProject(project.Id)
                .AddProject(newProjectInfo);
            var newProject = newSolution.GetProject(project.Id);

            if (project.DocumentIds.Count > 1)
                throw new NotSupportedException($"Calling {nameof(SetScriptMode)} after adding documents to the project is not currently supported.");

            if (project.AdditionalDocumentIds.Count > 0)
                throw new NotSupportedException($"Calling {nameof(SetScriptMode)} after adding additional documents to the project is not currently supported.");

            var newDocument = newProject.GetDocument(document.Id).WithText(_sourceText);

            _workspace.SetCurrentSolution(newDocument.Project.Solution);
            UpdateDocumentAfterSolutionChange();
        }

        private void EnsureDocumentUpToDate() {
            if (!_documentOutOfDate)
                return;

            var document = _document.WithText(_sourceText);
            ApplySolutionChange(document.Project.Solution);
        }

        private void ApplySolutionChange(Solution solution) {
            // ReSharper disable once PossibleNullReferenceException
            if (!_workspace.TryApplyChanges(solution))
                throw new Exception("Failed to apply changes to workspace.");
            UpdateDocumentAfterSolutionChange();
        }

        private void UpdateDocumentAfterSolutionChange() {
            _document = _workspace.CurrentSolution.GetDocument(_document.Id);
            _documentOutOfDate = false;
        }

        public async Task<IReadOnlyList<TextChange>> RollbackWorkspaceChangesAsync() {
            EnsureDocumentUpToDate();
            var oldProject = _document.Project;
            // ReSharper disable once PossibleNullReferenceException
            var newProject = _workspace.CurrentSolution.GetProject(Project.Id);
            if (newProject == oldProject)
                return NoTextChanges;

            var newText = await newProject.GetDocument(_document.Id).GetTextAsync().ConfigureAwait(false);
            _document = _workspace.SetCurrentSolution(oldProject.Solution).GetDocument(_document.Id);

            return newText.GetTextChanges(_sourceText);
        }

        public void Dispose() {
            _workspace?.Dispose();
        }
    }
}
