using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Completion;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Host.Mef;
using Microsoft.CodeAnalysis.Text;
using MirrorSharp.Internal.Results;

namespace MirrorSharp.Internal {
    public class WorkSession : IWorkSession {
        private static readonly MefHostServices HostServices = MefHostServices.Create(MefHostServices.DefaultAssemblies.AddRange(new[] {
            Assembly.Load(new AssemblyName("Microsoft.CodeAnalysis.Features")),
            Assembly.Load(new AssemblyName("Microsoft.CodeAnalysis.CSharp.Features"))
        }));
        private static readonly Task<TypeCharResult> TypeCharEmptyResultTask = Task.FromResult(new TypeCharResult());

        private readonly AdhocWorkspace _workspace;

        private readonly TextChange[] _oneTextChange = new TextChange[1];
        private Document _document;
        private SourceText _sourceText;
        private int _cursorPosition;
        private CompletionList _completionList;

        private readonly CompletionService _completionService;

        private static readonly ImmutableList<MetadataReference> DefaultAssemblyReferences = ImmutableList.Create<MetadataReference>(
            MetadataReference.CreateFromFile(typeof(object).GetTypeInfo().Assembly.Location)
        );

        private static readonly ImmutableList<AnalyzerReference> DefaultAnalyzerReferences = ImmutableList.Create<AnalyzerReference>(
            CreateAnalyzerReference("Microsoft.CodeAnalysis.CSharp.Features")
        );

        private readonly ImmutableArray<DiagnosticAnalyzer> _analyzers;

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
            _completionService = CompletionService.GetService(_document);
            if (_completionService == null)
                throw new Exception("Failed to retrieve the completion service.");

            _analyzers = ImmutableArray.CreateRange(project.AnalyzerReferences.SelectMany(r => r.GetAnalyzers("C#")));
        }

        private static AnalyzerFileReference CreateAnalyzerReference(string assemblyName) {
            var assembly = Assembly.Load(new AssemblyName(assemblyName));
            return new AnalyzerFileReference(assembly.Location, new PreloadedAnalyzerAssemblyLoader(assembly));
        }

        public void ReplaceText(int start, int length, string newText, int cursorPositionAfter) {
            _oneTextChange[0] = new TextChange(new TextSpan(start, length), newText);
            ApplyTextChanges(_oneTextChange);
            _cursorPosition = cursorPositionAfter;
        }

        private void ApplyTextChanges(IEnumerable<TextChange> changes) {
            _sourceText = _sourceText.WithChanges(changes);
            _document = _document.WithText(_sourceText);
        }

        public void MoveCursor(int cursorPosition) {
            _cursorPosition = cursorPosition;
        }

        public Task<TypeCharResult> TypeCharAsync(char @char, CancellationToken cancellationToken) {
            ReplaceText(_cursorPosition, 0, FastConvert.CharToString(@char), _cursorPosition + 1);
            if (!_completionService.ShouldTriggerCompletion(_sourceText, _cursorPosition, CompletionTrigger.CreateInsertionTrigger(@char)))
                return TypeCharEmptyResultTask;
            return CreateResultFromCompletionsAsync(cancellationToken);
        }

        public Task<CompletionChange> GetCompletionChangeAsync(int itemIndex, CancellationToken cancellationToken) {
            var item = _completionList.Items[itemIndex];
            return _completionService.GetChangeAsync(_document, item, cancellationToken: cancellationToken);
        }

        public async Task<SlowUpdateResult> GetSlowUpdateAsync(CancellationToken cancellationToken) {
            var compilation = await _document.Project.GetCompilationAsync(cancellationToken).ConfigureAwait(false);
            var diagnostics = await compilation.WithAnalyzers(_analyzers).GetAllDiagnosticsAsync(cancellationToken).ConfigureAwait(false);
            return new SlowUpdateResult(diagnostics);
        }

        private async Task<TypeCharResult> CreateResultFromCompletionsAsync(CancellationToken cancellationToken) {
            _completionList = await _completionService.GetCompletionsAsync(_document, _cursorPosition, cancellationToken: cancellationToken).ConfigureAwait(false);
            return new TypeCharResult(_completionList);
        }

        public SourceText SourceText => _sourceText;
        public int CursorPosition => _cursorPosition;

        public void Dispose() {
            _workspace.Dispose();
        }

        private class PreloadedAnalyzerAssemblyLoader : IAnalyzerAssemblyLoader {
            private readonly Assembly _assembly;

            public PreloadedAnalyzerAssemblyLoader(Assembly assembly) {
                _assembly = assembly;
            }

            public Assembly LoadFromPath(string fullPath) {
                return _assembly;
            }

            public void AddDependencyLocation(string fullPath) {
            }
        }
    }
}
