using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Completion;
using Microsoft.CodeAnalysis.Text;
using Microsoft.FSharp.Collections;
using Microsoft.FSharp.Compiler;
using Microsoft.FSharp.Compiler.SourceCodeServices;
using Microsoft.FSharp.Control;
using MirrorSharp.Internal.Abstraction;

namespace MirrorSharp.FSharp.Internal {
    internal class FSharpSession : ILanguageSession {
        private readonly FSharpChecker _checker;
        private readonly FSharpProjectOptions _projectOptions;

        private string _text;
        private IReadOnlyList<Line> _lastLineMap;
        private (FSharpParseFileResults, FSharpCheckFileAnswer)? _lastParseAndCheck;

        public FSharpSession(string text, ImmutableArray<string> assemblyReferencePaths, OptimizationLevel? optimizationLevel) {
            _checker = FSharpChecker.Create(null, null, null, false);
            _text = text;

            _projectOptions = new FSharpProjectOptions(
                "_",
                projectFileNames: new[] { "_.fs" },
                otherOptions: ConvertToOptions(assemblyReferencePaths, optimizationLevel),
                referencedProjects: Array.Empty<Tuple<string, FSharpProjectOptions>>(),
                isIncompleteTypeCheckEnvironment: true,
                useScriptResolutionRules: false,
                loadTime: DateTime.Now,
                unresolvedReferences: null,
                originalLoadReferences: FSharpList<Tuple<Range.range, string>>.Empty, 
                extraProjectInfo: null
            );
        }

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
            var success = result.answer as FSharpCheckFileAnswer.Succeeded;
            var diagnosticCount = result.parsed.Errors.Length + (success?.Item.Errors.Length ?? 0);
            if (diagnosticCount == 0)
                return ImmutableArray<Diagnostic>.Empty;
            
            var diagnostics = ImmutableArray.CreateBuilder<Diagnostic>(diagnosticCount);
            ConvertAndAddTo(diagnostics, result.parsed.Errors);

            if (success != null)
                ConvertAndAddTo(diagnostics, success.Item.Errors);

            return diagnostics.MoveToImmutable();
        }

        private async ValueTask<(FSharpParseFileResults parsed, FSharpCheckFileAnswer answer)> ParseAndCheckAsync(CancellationToken cancellationToken) {
            if (_lastParseAndCheck != null)
                return _lastParseAndCheck.Value;

            var tuple = await FSharpAsync.StartAsTask(
                _checker.ParseAndCheckFileInProject("_.fs", 0, _text, _projectOptions, null), null, cancellationToken
            ).ConfigureAwait(false);

            var valueTuple = (tuple.Item1, tuple.Item2);
            _lastParseAndCheck = valueTuple;
            return valueTuple;
        }

        private void ConvertAndAddTo(ImmutableArray<Diagnostic>.Builder diagnostics, FSharpErrorInfo[] errors) {
            var lineMap = GetLineMap();
            foreach (var error in errors) {
                var severity = ConvertToDiagnosticSeverity(error.Severity);

                var startOffset = GetOffset(error.StartLineAlternate, error.StartColumn, lineMap);
                var location = Location.Create(
                    "",
                    new TextSpan(
                        startOffset,
                        GetOffset(error.EndLineAlternate, error.EndColumn, lineMap) - startOffset
                    ),
                    new LinePositionSpan(
                        new LinePosition(error.StartLineAlternate, error.StartColumn),
                        new LinePosition(error.EndLineAlternate, error.EndColumn)
                    )
                );

                diagnostics.Add(Diagnostic.Create(
                    "FS", "Compiler",
                    error.Message,
                    severity, severity,
                    isEnabledByDefault: false,
                    warningLevel: severity == DiagnosticSeverity.Warning ? 1 : 0,
                    location: location
                ));
            }
        }

        private DiagnosticSeverity ConvertToDiagnosticSeverity(FSharpErrorSeverity errorSeverity) {
            return Equals(errorSeverity, FSharpErrorSeverity.Error) ? DiagnosticSeverity.Error : DiagnosticSeverity.Warning;
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
            if (!(result.answer is FSharpCheckFileAnswer.Succeeded success))
                return null;

            var lineMap = GetLineMap();
            var info = GetLineAndColumnInfo(cursorPosition, lineMap);

            var symbols = await FSharpAsync.StartAsTask(success.Item.GetDeclarationListSymbols(
                result.parsed, info.line.Number, info.column,
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

        private IReadOnlyList<Line> GetLineMap() {
            if (_lastLineMap != null)
                return _lastLineMap;

            var map = new List<Line>();
            var text = _text;
            var start = 0;
            var previous = '\0';
            for (var i = 0; i < text.Length; i++) {
                var @char = text[i];
                if (@char == '\r' || (previous != '\r' && @char == '\n'))
                    map.Add(new Line(map.Count + 1, start, i));
                if (previous == '\n' || (previous == '\r' && @char != '\n'))
                    start = i;
                previous = @char;
            }
            map.Add(new Line(map.Count + 1, start, text.Length));

            _lastLineMap = map;
            return map;
        }

        private (Line line, int column) GetLineAndColumnInfo(int offset, IReadOnlyList<Line> lineMap) {
            var line = lineMap[0];
            for (var i = 1; i < lineMap.Count; i++) {
                var nextLine = lineMap[i];
                if (offset < nextLine.Start)
                    break;
                line = nextLine;
            }
            return (line, offset - line.Start);
        }

        private int GetOffset(int line, int column, IReadOnlyList<Line> lineMap) {
            return lineMap[line - 1].Start + column;
        }

        public void Dispose() {
        }

        private struct Line {
            public Line(int number, int start, int end) {
                Number = number;
                Start = start;
                End = end;
            }

            public int Number { get; }
            public int Start { get; }
            public int End { get; }
            public int Length => End - Start;
        }

        private static class SymbolTags {
            private static ImmutableArray<string> Namespace { get; } = ImmutableArray.Create("Namespace");

            private static ImmutableArray<string> Delegate { get; } = ImmutableArray.Create("Delegate");
            private static ImmutableArray<string> Enum { get; } = ImmutableArray.Create("Enum");
            private static ImmutableArray<string> Union { get; } = ImmutableArray.Create("Union");
            private static ImmutableArray<string> Structure { get; } = ImmutableArray.Create("Structure");
            private static ImmutableArray<string> Class { get; } = ImmutableArray.Create("Class");
            private static ImmutableArray<string> Interface { get; } = ImmutableArray.Create("Interface");
            private static ImmutableArray<string> Module { get; } = ImmutableArray.Create("Module");
            
            private static ImmutableArray<string> Property { get; } = ImmutableArray.Create("Property");
            private static ImmutableArray<string> Method { get; } = ImmutableArray.Create("Method");
            private static ImmutableArray<string> Field { get; } = ImmutableArray.Create("Field");

            public static ImmutableArray<string> From(FSharpSymbol symbol) {
                switch (symbol) {
                    case FSharpField _: return Field;
                    case FSharpEntity e: return FromEntity(e);
                    case FSharpMemberOrFunctionOrValue m: {
                        if (m.IsProperty) return Property;
                        if (m.IsConstructor || m.FullType.IsFunctionType) return Method;
                        return ImmutableArray<string>.Empty;
                    }
                    default: return ImmutableArray<string>.Empty;
                }
            }

            private static ImmutableArray<string> FromEntity(FSharpEntity entity) {
                if (entity.IsNamespace) return Namespace;
                if (entity.IsClass) return Class;
                if (entity.IsInterface) return Interface;
                if (entity.IsDelegate) return Delegate;
                if (entity.IsEnum) return Enum;
                if (entity.IsFSharpUnion) return Union;
                if (entity.IsValueType) return Structure;
                if (entity.IsFSharpModule) return Module;
                if (entity.IsFSharpAbbreviation) return FromEntity(entity.AbbreviatedType.TypeDefinition);
                return ImmutableArray<string>.Empty;
            }
        }
    }
}