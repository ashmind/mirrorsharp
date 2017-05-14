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

        public FSharpSession(string text, ImmutableList<MetadataReference> metadataReferences) {
            _checker = FSharpChecker.Create(null, null, null, false);
            _text = text;

            var otherOptions = new List<string> { "--noframework" };
            foreach (var reference in metadataReferences) {
                switch (reference) {
                    case PortableExecutableReference pe:
                        // ReSharper disable once HeapView.ObjectAllocation (Unavoidable? Not worth caching)
                        otherOptions.Add("-r:" + pe.FilePath);
                        break;
                    default: throw new NotSupportedException($"Metadata reference type {reference.GetType()} is not supported.");
                }
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

            var diagnostics = ImmutableArray.CreateBuilder<Diagnostic>(parsed.Errors.Length + (checkSuccess?.Item.Errors.Length ?? 0));
            ConvertAndAddTo(diagnostics, parsed.Errors);

            if (checkSuccess != null)
                ConvertAndAddTo(diagnostics, checkSuccess.Item.Errors);

            return diagnostics.MoveToImmutable();
        }

        private void ConvertAndAddTo(ImmutableArray<Diagnostic>.Builder diagnostics, FSharpErrorInfo[] errors) {
            foreach (var error in errors) {
                var severity = ConvertToDiagnosticSeverity(error.Severity);
                var location = Location.Create(
                    "",
                    new TextSpan(0, 0),
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
                    warningLevel: 0,
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
        }

        public void Dispose() {
        }
    }
}