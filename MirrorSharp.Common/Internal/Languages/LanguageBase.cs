using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Host.Mef;
using MirrorSharp.Internal.Reflection;

namespace MirrorSharp.Internal.Languages {
    internal abstract class LanguageBase : ILanguage {
        protected LanguageBase(
            [NotNull] string name,
            [NotNull] string featuresAssemblyName,
            [NotNull] string workspacesAssemblyName,
            [NotNull] ParseOptions defaultParseOptions,
            [NotNull] CompilationOptions defaultCompilationOptions
        ) {
            Name = name;
            HostServices = MefHostServices.Create(new[] {
                Assembly.Load(new AssemblyName("Microsoft.CodeAnalysis.Workspaces")),
                Assembly.Load(new AssemblyName("Microsoft.CodeAnalysis.Features")),
                Assembly.Load(new AssemblyName(featuresAssemblyName)),
                Assembly.Load(new AssemblyName(workspacesAssemblyName)),
            });
            DefaultParseOptions = defaultParseOptions;
            DefaultCompilationOptions = defaultCompilationOptions;
            DefaultAssemblyReferences = ImmutableList.Create<MetadataReference>(
                MetadataReference.CreateFromFile(typeof(object).GetTypeInfo().Assembly.Location)
            );
            DefaultAnalyzerReferences = ImmutableList.Create<AnalyzerReference>(
                CreateAnalyzerReference(featuresAssemblyName)
            );
            DefaultCodeFixProvidersIndexedByDiagnosticIds = CreateDefaultCodeFixProviders();
            DefaultAnalyzers = ImmutableArray.CreateRange(
                DefaultAnalyzerReferences.SelectMany(r => r.GetAnalyzers(Name))
            );
            DefaultSignatureHelpProviders = CreateDefaultSignatureHelpProviders();
        }

        public string Name { get; }
        public MefHostServices HostServices { get; }
        public ParseOptions DefaultParseOptions { get; }
        public CompilationOptions DefaultCompilationOptions { get; }
        public ImmutableList<MetadataReference> DefaultAssemblyReferences { get; }
        public ImmutableList<AnalyzerReference> DefaultAnalyzerReferences { get; }
        public ImmutableArray<DiagnosticAnalyzer> DefaultAnalyzers { get; }
        public ImmutableDictionary<string, ImmutableArray<CodeFixProvider>> DefaultCodeFixProvidersIndexedByDiagnosticIds { get; }
        public ImmutableArray<ISignatureHelpProviderWrapper> DefaultSignatureHelpProviders { get; }

        private ImmutableArray<ISignatureHelpProviderWrapper> CreateDefaultSignatureHelpProviders() {
            return ImmutableArray.CreateRange(
                RoslynInternalCalls.GetSignatureHelpProvidersSlow(HostServices)
                    .Where(l => l.Metadata.Language == Name)
                    .Select(l => l.Value)
            );
        }

        private ImmutableDictionary<string, ImmutableArray<CodeFixProvider>> CreateDefaultCodeFixProviders() {
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

        private AnalyzerFileReference CreateAnalyzerReference(string assemblyName) {
            var assembly = Assembly.Load(new AssemblyName(assemblyName));
            return new AnalyzerFileReference(assembly.Location, new PreloadedAnalyzerAssemblyLoader(assembly));
        }
    }
}