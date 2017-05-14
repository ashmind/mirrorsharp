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
using Microsoft.CodeAnalysis.Text;
using MirrorSharp.Internal.Abstraction;
using MirrorSharp.Internal.Reflection;

namespace MirrorSharp.Internal.Roslyn {
    internal abstract class RoslynLanguageBase : ILanguage {
        private readonly MefHostServices _hostServices;
        private readonly ImmutableArray<ISignatureHelpProviderWrapper> _defaultSignatureHelpProviders;
        private readonly ImmutableDictionary<string, ImmutableArray<CodeFixProvider>> _defaultCodeFixProvidersIndexedByDiagnosticIds;
        private readonly ImmutableArray<DiagnosticAnalyzer> _defaultAnalyzers;
        private readonly ImmutableList<AnalyzerReference> _defaultAnalyzerReferences;
        private readonly ImmutableList<MetadataReference> _defaultAssemblyReferences;

        protected RoslynLanguageBase(
            [NotNull] string name,
            [NotNull] string featuresAssemblyName,
            [NotNull] string workspacesAssemblyName,
            [NotNull] ParseOptions defaultParseOptions,
            [NotNull] CompilationOptions defaultCompilationOptions
        ) {
            // ReSharper disable HeapView.BoxingAllocation
            Name = name;
            _hostServices = MefHostServices.Create(new[] {
                Assembly.Load(new AssemblyName("Microsoft.CodeAnalysis.Workspaces")),
                Assembly.Load(new AssemblyName("Microsoft.CodeAnalysis.Features")),
                Assembly.Load(new AssemblyName(featuresAssemblyName)),
                Assembly.Load(new AssemblyName(workspacesAssemblyName)),
            });
            DefaultParseOptions = defaultParseOptions;
            DefaultCompilationOptions = defaultCompilationOptions;
            _defaultAssemblyReferences = ImmutableList.Create<MetadataReference>(
                MetadataReference.CreateFromFile(typeof(object).GetTypeInfo().Assembly.Location)
            );
            _defaultAnalyzerReferences = ImmutableList.Create<AnalyzerReference>(
                CreateAnalyzerReference(featuresAssemblyName)
            );
            _defaultCodeFixProvidersIndexedByDiagnosticIds = CreateDefaultCodeFixProviders();
            _defaultAnalyzers = ImmutableArray.CreateRange(
                _defaultAnalyzerReferences.SelectMany(r => r.GetAnalyzers(Name))
            );
            _defaultSignatureHelpProviders = CreateDefaultSignatureHelpProviders();
            // ReSharper restore HeapView.BoxingAllocation
        }

        public string Name { get; }
        public ParseOptions DefaultParseOptions { get; }
        public CompilationOptions DefaultCompilationOptions { get; }

        public ILanguageSession CreateSession(string text, ParseOptions parseOptions, CompilationOptions compilationOptions, IReadOnlyCollection<MetadataReference> assemblyReferences) {
            var projectId = ProjectId.CreateNewId();
            var projectInfo = ProjectInfo.Create(
                projectId, VersionStamp.Create(), "_", "_", Name,
                parseOptions: parseOptions,
                compilationOptions: compilationOptions,
                metadataReferences: assemblyReferences ?? _defaultAssemblyReferences,
                analyzerReferences: _defaultAnalyzerReferences
            );
            
            return new RoslynSession(
                SourceText.From(text),
                projectInfo,
                _hostServices,
                _defaultAnalyzers,
                _defaultCodeFixProvidersIndexedByDiagnosticIds,
                _defaultSignatureHelpProviders
            );
        }

        private ImmutableArray<ISignatureHelpProviderWrapper> CreateDefaultSignatureHelpProviders() {
            return ImmutableArray.CreateRange(
                RoslynReflectionFast.GetSignatureHelpProvidersSlow(_hostServices)
                    .Where(l => l.Metadata.Language == Name)
                    .Select(l => l.Value)
            );
        }

        private ImmutableDictionary<string, ImmutableArray<CodeFixProvider>> CreateDefaultCodeFixProviders() {
            var codeFixProviderTypes = _defaultAnalyzerReferences
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