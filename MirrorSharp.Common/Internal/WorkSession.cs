using System;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Completion;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Host.Mef;
using Microsoft.CodeAnalysis.Text;

namespace MirrorSharp.Internal {
    public class WorkSession {
        private static readonly MefHostServices HostServices = MefHostServices.Create(MefHostServices.DefaultAssemblies.AddRange(new[] {
            Assembly.Load(new AssemblyName("Microsoft.CodeAnalysis.Features")),
            Assembly.Load(new AssemblyName("Microsoft.CodeAnalysis.CSharp.Features"))
        }));

        private readonly AdhocWorkspace _workspace;

        private SourceText _sourceText;
        private bool _documentOutOfDate;
        private Document _document;

        private static readonly ImmutableList<MetadataReference> DefaultAssemblyReferences = ImmutableList.Create<MetadataReference>(
            MetadataReference.CreateFromFile(typeof(object).GetTypeInfo().Assembly.Location)
        );

        private static readonly ImmutableList<AnalyzerReference> DefaultAnalyzerReferences = ImmutableList.Create<AnalyzerReference>(
            CreateAnalyzerReference("Microsoft.CodeAnalysis.CSharp.Features")
        );

        public WorkSession() {
            _workspace = new AdhocWorkspace(HostServices);
            var projectId = ProjectId.CreateNewId();
            var project = _workspace.AddProject(ProjectInfo.Create(
                projectId, VersionStamp.Create(), "_", "_", "C#",
                compilationOptions: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary),
                metadataReferences: DefaultAssemblyReferences,
                analyzerReferences: DefaultAnalyzerReferences
            ));
            _sourceText = SourceText.From("");
            _document = _workspace.AddDocument(projectId, "_", _sourceText);
            _workspace.OpenDocument(_document.Id);
            CompletionService = CompletionService.GetService(_document);
            if (CompletionService == null)
                throw new Exception("Failed to retrieve the completion service.");

            Analyzers = ImmutableArray.CreateRange(project.AnalyzerReferences.SelectMany(r => r.GetAnalyzers("C#")));
            Buffers = new Buffers();
        }

        private static AnalyzerFileReference CreateAnalyzerReference(string assemblyName) {
            var assembly = Assembly.Load(new AssemblyName(assemblyName));
            return new AnalyzerFileReference(assembly.Location, new PreloadedAnalyzerAssemblyLoader(assembly));
        }

        public int CursorPosition { get; set; }

        public SourceText SourceText {
            get { return _sourceText; }
            set {
                _sourceText = value;
                _documentOutOfDate = true;
            }
        }

        public Document Document {
            get {
                if (_documentOutOfDate) {
                    _document = _document.WithText(_sourceText);
                    _documentOutOfDate = false;
                }
                return _document;
            }
        }

        [NotNull] public CompletionService CompletionService { get; }
        [CanBeNull] public CompletionList CurrentCompletionList { get; set; }

        public Project Project => Document.Project;
        public ImmutableArray<DiagnosticAnalyzer> Analyzers { get; }

        public Buffers Buffers { get; }

        public void Dispose() {
            _workspace.Dispose();
        }
    }
}
