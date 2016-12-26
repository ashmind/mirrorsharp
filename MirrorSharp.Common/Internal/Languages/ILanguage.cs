using System.Collections.Immutable;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Host.Mef;
using MirrorSharp.Internal.Reflection;

namespace MirrorSharp.Internal.Languages {
    internal interface ILanguage {
        [NotNull] string Name { get; }
        [NotNull] ParseOptions DefaultParseOptions { get; }
        [NotNull] CompilationOptions DefaultCompilationOptions { get; }
        [NotNull] [ItemNotNull] ImmutableList<MetadataReference> DefaultAssemblyReferences { get; }
        [NotNull] [ItemNotNull] ImmutableList<AnalyzerReference> DefaultAnalyzerReferences { get; }
        [ItemNotNull] ImmutableArray<DiagnosticAnalyzer> DefaultAnalyzers { get; }
        [NotNull] ImmutableDictionary<string, ImmutableArray<CodeFixProvider>> DefaultCodeFixProvidersIndexedByDiagnosticIds { get; }
        [ItemNotNull] ImmutableArray<ISignatureHelpProviderWrapper> DefaultSignatureHelpProviders { get; }
        [NotNull] MefHostServices HostServices { get; }
    }
}