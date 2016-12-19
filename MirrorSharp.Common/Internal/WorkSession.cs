using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Completion;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Host.Mef;
using Microsoft.CodeAnalysis.Text;
using MirrorSharp.Internal.Reflection;

namespace MirrorSharp.Internal {
    public class WorkSession {
        private static readonly TextChange[] NoTextChanges = new TextChange[0];
        private static readonly MefHostServices HostServices = MefHostServices.Create(MefHostServices.DefaultAssemblies.AddRange(new[] {
            Assembly.Load(new AssemblyName("Microsoft.CodeAnalysis.Features")),
            Assembly.Load(new AssemblyName("Microsoft.CodeAnalysis.CSharp.Features"))
        }));

        private readonly CustomWorkspace _workspace;

        private SourceText _sourceText;
        private bool _documentOutOfDate;
        private Document _document;

        private static readonly ImmutableList<MetadataReference> DefaultAssemblyReferences = ImmutableList.Create<MetadataReference>(
            MetadataReference.CreateFromFile(typeof(object).GetTypeInfo().Assembly.Location)
        );

        private static readonly ImmutableList<AnalyzerReference> DefaultAnalyzerReferences = ImmutableList.Create<AnalyzerReference>(
            CreateAnalyzerReference("Microsoft.CodeAnalysis.CSharp.Features")
        );

        private static readonly ImmutableArray<DiagnosticAnalyzer> DefaultAnalyzers = CreateDefaultAnalyzers();
        private static readonly ImmutableDictionary<string, ImmutableArray<CodeFixProvider>> DefaultCodeFixProviders = CreateDefaultCodeFixProviders();
        private static readonly ImmutableArray<ISignatureHelpProviderWrapper> DefaultSignatureHelpProviders = CreateDefaultSignatureHelpProviders();

        public WorkSession() {
            var projectId = ProjectId.CreateNewId();
            var projectInfo = ProjectInfo.Create(
                projectId, VersionStamp.Create(), "_", "_", "C#",
                compilationOptions: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary),
                metadataReferences: DefaultAssemblyReferences,
                analyzerReferences: DefaultAnalyzerReferences
            );
            var documentId = DocumentId.CreateNewId(projectId);
            _sourceText = SourceText.From("");

            _workspace = new CustomWorkspace(HostServices);
            var solution = _workspace.CurrentSolution
                .AddProject(projectInfo)
                .AddDocument(documentId, "_", _sourceText);
            solution = _workspace.SetCurrentSolution(solution);
            _workspace.OpenDocument(documentId);
            _document = solution.GetDocument(documentId);
            CompletionService = CompletionService.GetService(_document);
            if (CompletionService == null)
                throw new Exception("Failed to retrieve the completion service.");

            Analyzers = DefaultAnalyzers;
            CodeFixProviders = DefaultCodeFixProviders;
            SignatureHelpProviders = DefaultSignatureHelpProviders;
        }

        private static AnalyzerFileReference CreateAnalyzerReference(string assemblyName) {
            var assembly = Assembly.Load(new AssemblyName(assemblyName));
            return new AnalyzerFileReference(assembly.Location, new PreloadedAnalyzerAssemblyLoader(assembly));
        }

        private static ImmutableDictionary<string, ImmutableArray<CodeFixProvider>> CreateDefaultCodeFixProviders() {
            var codeFixProviderTypes = DefaultAnalyzerReferences
                .OfType<AnalyzerFileReference>()
                .Select(a => a.GetAssembly())
                .SelectMany(a => a.DefinedTypes)
                .Where(t => t.IsDefined(typeof(ExportCodeFixProviderAttribute)));

            var providersByDiagnosticIds = new Dictionary<string, IList<CodeFixProvider>>();
            foreach (var type in codeFixProviderTypes) {
                var provider = (CodeFixProvider)Activator.CreateInstance(type.AsType());

                foreach (var id in provider.FixableDiagnosticIds) {
                    IList<CodeFixProvider> list;
                    if (!providersByDiagnosticIds.TryGetValue(id, out list)) {
                        list = new List<CodeFixProvider>();
                        providersByDiagnosticIds.Add(id, list);
                    }
                    list.Add(provider);
                }
            }
            return ImmutableDictionary.CreateRange(
                providersByDiagnosticIds.Select(p => new KeyValuePair<string, ImmutableArray<CodeFixProvider>>(p.Key, ImmutableArray.CreateRange(p.Value)))
            );
        }

        private static ImmutableArray<DiagnosticAnalyzer> CreateDefaultAnalyzers() {
            return ImmutableArray.CreateRange(DefaultAnalyzerReferences.SelectMany(r => r.GetAnalyzers("C#")));
        }

        private static ImmutableArray<ISignatureHelpProviderWrapper> CreateDefaultSignatureHelpProviders() {
            return ImmutableArray.CreateRange(
                RoslynInternalCalls.GetSignatureHelpProvidersSlow(HostServices)
                    .Where(l => l.Metadata.Language == "C#")
                    .Select(l => l.Value)
            );
        }

        public int CursorPosition { get; set; }

        public SourceText SourceText {
            get { return _sourceText; }
            set {
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

        [NotNull] public CompletionService CompletionService { get; }
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
        public ImmutableArray<DiagnosticAnalyzer> Analyzers { get; }
        public ImmutableDictionary<string, ImmutableArray<CodeFixProvider>> CodeFixProviders { get; }
        internal ImmutableArray<ISignatureHelpProviderWrapper> SignatureHelpProviders { get; }

        private void EnsureDocumentUpToDate() {
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

        public void Dispose() {
            _workspace.Dispose();
        }
    }
}

