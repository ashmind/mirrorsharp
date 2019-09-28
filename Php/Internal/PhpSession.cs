extern alias peachpie;
using PeachpieRoslyn = peachpie::Microsoft.CodeAnalysis;

using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Completion;
using Microsoft.CodeAnalysis.Text;
using MirrorSharp.Internal.Abstraction;
using MirrorSharp.Php.Advanced;
using Pchp.CodeAnalysis;

using SourceText = peachpie::Microsoft.CodeAnalysis.Text.SourceText;

namespace MirrorSharp.Php.Internal {
    internal class PhpSession : ILanguageSessionInternal, IPhpSession {
        private const string AssemblyName = "app";
        private const string ScriptFileName = "index.php";

        /// <summary>Helper base compilation object used to cache the parsed references.</summary>
        private static PhpCompilation CoreCompilation { get; }

        static PhpSession() {
            CoreCompilation = PhpCompilation.Create(
                assemblyName: AssemblyName,
                syntaxTrees: ImmutableArray<PhpSyntaxTree>.Empty,
                references: MirrorSharpPhpOptions.AssemblyReferencePaths.Select(path => PeachpieRoslyn.MetadataReference.CreateFromFile(path)),
                options: new PhpCompilationOptions(
                    outputKind: PeachpieRoslyn.OutputKind.ConsoleApplication,
                    baseDirectory: Environment.CurrentDirectory,
                    sdkDirectory: null
                )
            );

            // Bind reference manager, cache all references
            var hlp = CoreCompilation.Assembly;
        }

        private string _text;
        private MirrorSharpPhpOptions _options;

        public PhpSession(string text, MirrorSharpPhpOptions options) {
            _text = text;
            _options = options;

            var syntaxTree = PhpSyntaxTree.ParseCode(text, PhpParseOptions.Default, PhpParseOptions.Default, ScriptFileName);
            Compilation = (PhpCompilation)CoreCompilation.AddSyntaxTrees(syntaxTree);

            if (options.Debug == false) {
                Compilation = Compilation.WithPhpOptions(Compilation.Options.WithOptimizationLevel(PeachpieRoslyn.OptimizationLevel.Release));
            }
        }

        public PhpCompilation Compilation { get; set; }

        public string GetText() => _text;

        public void ReplaceText(string? newText, int start = 0, int? length = null) {
            if (length > 0)
                _text = _text.Remove(start, length.Value);
            if (newText?.Length > 0)
                _text = _text.Insert(start, newText);

            var syntaxTree = PhpSyntaxTree.ParseCode(_text, PhpParseOptions.Default, PhpParseOptions.Default, ScriptFileName);
            Compilation = (PhpCompilation)Compilation.ReplaceSyntaxTree(Compilation.SyntaxTrees.Single(), syntaxTree);
        }

        public async Task<ImmutableArray<Diagnostic>> GetDiagnosticsAsync(CancellationToken cancellationToken) {
            var parseDiags = Compilation.GetParseDiagnostics();
            if (parseDiags.Length > 0) {
                return parseDiags
                    .Select(diagnostic => diagnostic.ToStandardRoslyn())
                    .ToImmutableArray();
            } else {
                return (await Compilation.BindAndAnalyseTask().ConfigureAwait(false))
                    .Select(diagnostic => diagnostic.ToStandardRoslyn())
                    .ToImmutableArray();
            }
        }

        public bool ShouldTriggerCompletion(int cursorPosition, CompletionTrigger trigger) {
            return false; // not supported yet
        }

        public Task<CompletionList?> GetCompletionsAsync(int cursorPosition, CompletionTrigger trigger, CancellationToken cancellationToken) {
            return Task.FromResult<CompletionList?>(CompletionList.Empty); // not supported yet
        }

        public Task<CompletionDescription?> GetCompletionDescriptionAsync(CompletionItem item, CancellationToken cancellationToken) {
            throw new NotSupportedException(); // not supported yet
        }

        public Task<CompletionChange> GetCompletionChangeAsync(TextSpan completionSpan, CompletionItem item, CancellationToken cancellationToken) {
            throw new NotSupportedException(); // not supported yet
        }

        public void Dispose() {}
    }
}
