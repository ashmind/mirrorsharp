using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Microsoft.FSharp.Collections;
using Microsoft.FSharp.Compiler;
using MirrorSharp.Internal.Abstraction;
using Microsoft.FSharp.Compiler.SourceCodeServices;
using Microsoft.FSharp.Control;

namespace MirrorSharp.FSharp {
    internal class FSharpSession : ILanguageSession {
        private string _text;

        private readonly FSharpChecker _checker;
        private readonly FSharpProjectOptions _projectOptions;

        public FSharpSession(string text, ImmutableArray<string> assemblyReferencePaths) {
            _checker = FSharpChecker.Create(null, null, null, false);
            _text = text;

            var otherOptions = new List<string> { "--noframework" };
            foreach (var path in assemblyReferencePaths) {
                // ReSharper disable once HeapView.ObjectAllocation (Not worth fixing for now)
                otherOptions.Add("-r:" + path);
            }

            _projectOptions = new FSharpProjectOptions(
                "_",
                projectFileNames: new[] { "_.fs" },
                otherOptions: otherOptions.ToArray(),
                referencedProjects: Array.Empty<Tuple<string, FSharpProjectOptions>>(),
                isIncompleteTypeCheckEnvironment: true,
                useScriptResolutionRules: false,
                loadTime: DateTime.Now,
                unresolvedReferences: null,
                originalLoadReferences: FSharpList<Tuple<Range.range, string>>.Empty, 
                extraProjectInfo: null
            );
        }

        public async Task<ImmutableArray<Diagnostic>> GetDiagnosticsAsync(CancellationToken cancellationToken) {
            var (parsed, check) = await FSharpAsync.StartAsTask(
                _checker.ParseAndCheckFileInProject("_.fs", 0, _text, _projectOptions, null), null, cancellationToken
            ).ConfigureAwait(false);
            var checkSuccess = check as FSharpCheckFileAnswer.Succeeded;
            var diagnosticCount = parsed.Errors.Length + (checkSuccess?.Item.Errors.Length ?? 0);
            if (diagnosticCount == 0)
                return ImmutableArray<Diagnostic>.Empty;

            var lineOffsets = MapLineOffsets(_text);
            var diagnostics = ImmutableArray.CreateBuilder<Diagnostic>(diagnosticCount);
            ConvertAndAddTo(diagnostics, parsed.Errors, lineOffsets);

            if (checkSuccess != null)
                ConvertAndAddTo(diagnostics, checkSuccess.Item.Errors, lineOffsets);

            return diagnostics.MoveToImmutable();
        }

        private IReadOnlyList<int> MapLineOffsets(string text) {
            var offsets = new List<int>();

            var previous = '\0';
            for (var i = 0; i < text.Length; i++) {
                var @char = text[i];
                if (previous == '\n' || (previous == '\r' && @char != '\n'))
                    offsets.Add(i);
                previous = @char;
            }

            return offsets;
        }

        private void ConvertAndAddTo(ImmutableArray<Diagnostic>.Builder diagnostics, FSharpErrorInfo[] errors, IReadOnlyList<int> lineOffsets) {
            foreach (var error in errors) {
                var severity = ConvertToDiagnosticSeverity(error.Severity);

                var startOffset = GetOffset(error.StartLineAlternate, error.StartColumn, lineOffsets);
                var location = Location.Create(
                    "",
                    new TextSpan(
                        startOffset,
                        GetOffset(error.EndLineAlternate, error.EndColumn, lineOffsets) - startOffset
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

        private int GetOffset(int line, int column, IReadOnlyList<int> lineOffsets) {
            var lineOffset = line > 1 ? lineOffsets[line - 2] : 0;
            return lineOffset + column;
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
        }

        public void Dispose() {
        }
    }
}