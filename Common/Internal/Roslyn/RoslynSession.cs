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
using Microsoft.CodeAnalysis.Text;
using MirrorSharp.Advanced;
using MirrorSharp.Internal.Abstraction;
using MirrorSharp.Internal.Reflection;

namespace MirrorSharp.Internal.Roslyn {
    internal class RoslynSession : ILanguageSession, IRoslynSession {
        private static readonly TextChange[] NoTextChanges = new TextChange[0];

        private readonly TextChange[] _oneTextChange = new TextChange[1];
        private readonly CustomWorkspace _workspace;

        private bool _documentOutOfDate;
        private Document _document;
        private SourceText _sourceText;

        public RoslynSession(SourceText sourceText, ProjectInfo projectInfo, MefHostServices hostServices, ImmutableArray<DiagnosticAnalyzer> analyzers, ImmutableDictionary<string, ImmutableArray<CodeFixProvider>> codeFixProviders, ImmutableArray<ISignatureHelpProviderWrapper> signatureHelpProviders) {
            _workspace = new CustomWorkspace(hostServices);
            _sourceText = sourceText;
            _document = CreateProjectAndOpenNewDocument(_workspace, projectInfo, sourceText);
            Completion = CreateCompletion(_document);

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

        private Completion CreateCompletion(Document document) {
            var completionService = CompletionService.GetService(document);
            if (completionService == null)
                throw new Exception("Failed to retrieve the completion service.");
            return new Completion(completionService);
        }

        public string GetText() => SourceText.ToString();
        public void ReplaceText(string newText, int start = 0, int? length = null) {
            var finalLength = length ?? SourceText.Length - start;
            _oneTextChange[0] = new TextChange(new TextSpan(start, finalLength), newText);
            SourceText = SourceText.WithChanges(_oneTextChange);
        }

        public async Task<ImmutableArray<Diagnostic>> GetDiagnosticsAsync(CancellationToken cancellationToken) {
            var compilation = await Project.GetCompilationAsync(cancellationToken).ConfigureAwait(false);
            return await compilation.WithAnalyzers(Analyzers).GetAllDiagnosticsAsync(cancellationToken).ConfigureAwait(false);
        }

        [NotNull] public IList<CodeAction> CurrentCodeActions { get; } = new List<CodeAction>();

        [NotNull]
        public CustomWorkspace Workspace {
            get {
                EnsureDocumentUpToDate();
                return _workspace;
            }
        }

        public Project Project => Document.Project;

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

        [NotNull] public Completion Completion { get; }
        [CanBeNull] public CurrentSignatureHelp? CurrentSignatureHelp { get; set; }

        public ImmutableArray<DiagnosticAnalyzer> Analyzers { get; }
        public ImmutableDictionary<string, ImmutableArray<CodeFixProvider>> CodeFixProviders { get; }
        public ImmutableArray<ISignatureHelpProviderWrapper> SignatureHelpProviders { get; }

        private void EnsureDocumentUpToDate() {
            if (!_documentOutOfDate)
                return;

            var document = _document.WithText(_sourceText);
            // ReSharper disable once PossibleNullReferenceException
            if (!_workspace.TryApplyChanges(document.Project.Solution))
                throw new Exception("Failed to apply changes to workspace.");
            _document = _workspace.CurrentSolution.GetDocument(document.Id);
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
