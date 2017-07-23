using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Completion;
using Microsoft.CodeAnalysis.Text;
using Microsoft.FSharp.Collections;
using Microsoft.FSharp.Compiler;
using Microsoft.FSharp.Compiler.SourceCodeServices;
using Microsoft.FSharp.Control;
using MirrorSharp.FSharp.Advanced;
using MirrorSharp.Internal.Abstraction;

namespace MirrorSharp.FSharp.Internal {
    internal class FSharpSession : ILanguageSession, IFSharpSession {
        private string _text;
        private IVirtualFileInternal _virtualSourceFile;
        [CanBeNull] private LineColumnMap _lastLineMap;
        [CanBeNull] private FSharpParseAndCheckResults _lastParseAndCheck;

        public FSharpSession(string text, ImmutableArray<string> assemblyReferencePaths, OptimizationLevel? optimizationLevel) {
            _text = text;
            _virtualSourceFile = new VirtualSourceFile(() => _text);

            Checker = FSharpChecker.Create(
                null,
                keepAssemblyContents: true,
                keepAllBackgroundResolutions: true,
                msbuildEnabled: false               
            );
            AssemblyReferencePaths = assemblyReferencePaths;
            AssemblyReferencePathsAsFSharpList = ToFSharpList(assemblyReferencePaths);
            ProjectOptions = new FSharpProjectOptions(
                "_",
                projectFileNames: new[] { _virtualSourceFile.Name },
                otherOptions: ConvertToOptions(assemblyReferencePaths, optimizationLevel),
                referencedProjects: Array.Empty<Tuple<string, FSharpProjectOptions>>(),
                isIncompleteTypeCheckEnvironment: true,
                useScriptResolutionRules: false,
                loadTime: DateTime.Now,
                unresolvedReferences: null,
                originalLoadReferences: FSharpList<Tuple<Range.range, string>>.Empty, 
                extraProjectInfo: null
            );

            CustomFileSystem.Instance.RegisterVirtualFile(_virtualSourceFile);
        }

        private FSharpList<string> ToFSharpList(ImmutableArray<string> assemblyReferencePaths) {
            var list = FSharpList<string>.Empty;
            for (var i = assemblyReferencePaths.Length - 1; i >= 0; i--) {
                list = FSharpList<string>.Cons(assemblyReferencePaths[i], list);
            }
            return list;
        }

        public FSharpChecker Checker { get; }
        public FSharpProjectOptions ProjectOptions { get; }
        public ImmutableArray<string> AssemblyReferencePaths { get; }
        public FSharpList<string> AssemblyReferencePathsAsFSharpList { get; }

        private static string[] ConvertToOptions(ImmutableArray<string> assemblyReferencePaths, OptimizationLevel? optimizationLevel) {
            var options = new List<string> {"--noframework"};
            if (optimizationLevel == OptimizationLevel.Release) {
                options.Add("--debug-");
                options.Add("--optimize+");
            }
            else if (optimizationLevel == OptimizationLevel.Debug) {
                options.Add("--debug+");
                options.Add("--optimize-");
            }
            foreach (var path in assemblyReferencePaths) {
                // ReSharper disable once HeapView.ObjectAllocation (Not worth fixing for now)
                options.Add("-r:" + path);
            }
            return options.ToArray();
        }

        public async Task<ImmutableArray<Diagnostic>> GetDiagnosticsAsync(CancellationToken cancellationToken) {
            var result = await ParseAndCheckAsync(cancellationToken).ConfigureAwait(false);
            var success = result.CheckAnswer as FSharpCheckFileAnswer.Succeeded;
            var diagnosticCount = result.ParseResults.Errors.Length + (success?.Item.Errors.Length ?? 0);
            if (diagnosticCount == 0)
                return ImmutableArray<Diagnostic>.Empty;
            
            var diagnostics = ImmutableArray.CreateBuilder<Diagnostic>(diagnosticCount);
            ConvertAndAddTo(diagnostics, result.ParseResults.Errors);

            if (success != null)
                ConvertAndAddTo(diagnostics, success.Item.Errors);

            return diagnostics.MoveToImmutable();
        }
        
        public async ValueTask<FSharpParseAndCheckResults> ParseAndCheckAsync(CancellationToken cancellationToken) {
            if (_lastParseAndCheck != null)
                return _lastParseAndCheck;

            var tuple = await FSharpAsync.StartAsTask(
                Checker.ParseAndCheckFileInProject(_virtualSourceFile.Name, 0, _text, ProjectOptions, null), null, cancellationToken
            ).ConfigureAwait(false);

            _lastParseAndCheck = new FSharpParseAndCheckResults(tuple.Item1, tuple.Item2);
            return _lastParseAndCheck;
        }

        public FSharpParseFileResults GetLastParseResults() {
            return _lastParseAndCheck?.ParseResults;
        }

        public FSharpCheckFileAnswer GetLastCheckAnswer() {
            return _lastParseAndCheck?.CheckAnswer;
        }

        private void ConvertAndAddTo(ImmutableArray<Diagnostic>.Builder diagnostics, FSharpErrorInfo[] errors) {
            foreach (var error in errors) {
                diagnostics.Add(ConvertToDiagnostic(error));
            }
        }

        public Diagnostic ConvertToDiagnostic(FSharpErrorInfo error) {
            Argument.NotNull(nameof(error), error);

            var lineMap = GetLineMap();
            var severity = error.Severity.Tag == FSharpErrorSeverity.Tags.Error ? DiagnosticSeverity.Error : DiagnosticSeverity.Warning;

            var startOffset = lineMap.GetOffset(error.StartLineAlternate, error.StartColumn);
            var location = Location.Create(
                "",
                new TextSpan(
                    startOffset,
                    lineMap.GetOffset(error.EndLineAlternate, error.EndColumn) - startOffset
                ),
                new LinePositionSpan(
                    new LinePosition(error.StartLineAlternate, error.StartColumn),
                    new LinePosition(error.EndLineAlternate, error.EndColumn)
                )
            );

            return Diagnostic.Create(
                "FS", "Compiler",
                error.Message,
                severity, severity,
                isEnabledByDefault: false,
                warningLevel: severity == DiagnosticSeverity.Warning ? 1 : 0,
                location: location
            );
        }

        public string GetText() {
            return _text;
        }

        public void ReplaceText(string newText, int start = 0, int? length = null) {
            if (length > 0)
                _text = _text.Remove(start, length.Value);
            if (newText?.Length > 0)
                _text = _text.Insert(start, newText);

            _lastParseAndCheck = null;
            _lastLineMap = null;
        }

        public bool ShouldTriggerCompletion(int cursorPosition, CompletionTrigger trigger) {
            return (trigger.Kind == CompletionTriggerKind.Insertion && trigger.Character == '.')
                || trigger.Kind == CompletionTriggerKind.Other;
        }

        public async Task<CompletionList> GetCompletionsAsync(int cursorPosition, CompletionTrigger trigger, CancellationToken cancellationToken) {
            var result = await ParseAndCheckAsync(cancellationToken);
            if (!(result.CheckAnswer is FSharpCheckFileAnswer.Succeeded success))
                return null;

            var info = GetLineMap().GetLineAndColumn(cursorPosition);

            var symbols = await FSharpAsync.StartAsTask(success.Item.GetDeclarationListSymbols(
                result.ParseResults, info.line.Number, info.column,
                _text.Substring(info.line.Start, info.line.Length),
                FSharpList<string>.Empty,
                "", null
            ), null, cancellationToken);
            if (symbols.IsEmpty)
                return null;

            return CompletionList.Create(
                new TextSpan(cursorPosition, 0),
                ConvertToCompletionItems(symbols)
            );
        }

        private ImmutableArray<CompletionItem> ConvertToCompletionItems(FSharpList<FSharpList<FSharpSymbolUse>> symbols) {
            var items = ImmutableArray.CreateBuilder<CompletionItem>(symbols.Length);
            foreach (var list in symbols) {
                var use = list.Head;
                items.Add(CompletionItem.Create(
                    use.Symbol.DisplayName,
                    tags: SymbolTags.From(use.Symbol)
                ));
            }
            return items.MoveToImmutable();
        }

        public Task<CompletionChange> GetCompletionChangeAsync(TextSpan completionSpan, CompletionItem item, CancellationToken cancellationToken) {
            return Task.FromResult(CompletionChange.Create(new TextChange(completionSpan, item.DisplayText)));
        }

        public int ConvertToOffset(int line, int column) {
            Argument.PositiveOrZero(nameof(line), line);
            Argument.PositiveOrZero(nameof(column), column);
            return GetLineMap().GetOffset(line, column);
        }

        private LineColumnMap GetLineMap() {
            if (_lastLineMap == null)
                _lastLineMap = LineColumnMap.BuildFor(_text);
            return _lastLineMap;
        }

        public void Dispose() {
            CustomFileSystem.Instance.RemoveVirtualFile(_virtualSourceFile);
        }
    }
}