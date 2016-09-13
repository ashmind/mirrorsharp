using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Completion;
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

        //private readonly Task _compilationLoopTask;
        private readonly CancellationTokenSource _disposing;

        public WorkSession() {
            _disposing = new CancellationTokenSource();
            //_compilationLoopTask = Task.Run(CompilationLoop);

            _workspace = new AdhocWorkspace(HostServices);

            var project = _workspace.AddProject("_", "C#");
            _sourceText = SourceText.From("");
            _document = project.AddDocument("_", _sourceText);

            _completionService = CompletionService.GetService(_document);
            if (_completionService == null)
                throw new Exception("Failed to retrieve the completion service.");
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

        public Task<TypeCharResult> TypeCharAsync(char @char) {
            ReplaceText(_cursorPosition, 0, FastConvert.CharToString(@char), _cursorPosition + 1);
            if (!_completionService.ShouldTriggerCompletion(_sourceText, _cursorPosition, CompletionTrigger.CreateInsertionTrigger(@char)))
                return TypeCharEmptyResultTask;

            return CreateResultFromCompletionsAsync();
        }

        public Task<CompletionChange> GetCompletionChangeAsync(int itemIndex) {
            var item = _completionList.Items[itemIndex];
            return _completionService.GetChangeAsync(_document, item);
        }

        private async Task<TypeCharResult> CreateResultFromCompletionsAsync() {
            _completionList = await _completionService.GetCompletionsAsync(_document, _cursorPosition).ConfigureAwait(false);
            return new TypeCharResult(_completionList);
        }

        /*private async Task CompilationLoop() {
            var lastText = _text;
            var lastCompilation = CSharpCompilation.Create("_", new[] {_document.GetSyntaxTreeAsync() });

            while (!_disposing.IsCancellationRequested) {
                if (_text != lastText) {
                    _completionService.ShouldTriggerCompletion()

                    var compilation = lastCompilation.ReplaceSyntaxTree(lastTree, tree);
                    _callback(compilation.GetDiagnostics().Select(d => d.GetMessage()).ToArray());
                    lastText = _text;
                    lastCompilation = compilation;
                }

                try {
                    await Task.Delay(500, _disposing.Token).ConfigureAwait(false);
                }
                catch (OperationCanceledException) {}
            }

            / *return _compilation.GetDiagnostics().Select(d => new {
                Message = d.GetMessage()
            });* /
        }*/

        public SourceText SourceText => _sourceText;
        public int CursorPosition => _cursorPosition;

        public async Task DisposeAsync() {
            using (_disposing) {
                _disposing.Cancel();
                //await _compilationLoopTask.ConfigureAwait(false);
                _workspace.Dispose();
            }
        }
    }
}
