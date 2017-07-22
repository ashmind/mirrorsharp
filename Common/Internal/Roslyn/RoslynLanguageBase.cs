using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Text;
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
        private readonly ParseOptions _parseOptions;
        private readonly CompilationOptions _compilationOptions;
        private readonly ImmutableList<MetadataReference> _metadataReferences;
        private readonly ImmutableArray<ISignatureHelpProviderWrapper> _defaultSignatureHelpProviders;
        private readonly ImmutableDictionary<string, ImmutableArray<CodeFixProvider>> _defaultCodeFixProvidersIndexedByDiagnosticIds;
        private readonly ImmutableArray<DiagnosticAnalyzer> _defaultAnalyzers;
        private readonly ImmutableList<AnalyzerReference> _defaultAnalyzerReferences;

        protected RoslynLanguageBase(
            [NotNull] string name,
            [NotNull] string featuresAssemblyName,
            [NotNull] string workspacesAssemblyName,
            [NotNull] ParseOptions parseOptions,
            [NotNull] CompilationOptions compilationOptions,
            [NotNull] ImmutableList<MetadataReference> metadataReferences
        ) {
            // ReSharper disable HeapView.BoxingAllocation
            Name = name;
            _hostServices = MefHostServices.Create(new[] {
                Assembly.Load(new AssemblyName("Microsoft.CodeAnalysis.Workspaces")),
                Assembly.Load(new AssemblyName("Microsoft.CodeAnalysis.Features")),
                Assembly.Load(new AssemblyName(featuresAssemblyName)),
                Assembly.Load(new AssemblyName(workspacesAssemblyName)),
            });
            _parseOptions = parseOptions;
            _compilationOptions = compilationOptions;
            _metadataReferences = metadataReferences;
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

        public ILanguageSession CreateSession(string text, OptimizationLevel? optimizationLevel) {
            var projectId = ProjectId.CreateNewId();

            var compilationOptions = _compilationOptions;
            if (optimizationLevel != null)
                compilationOptions = compilationOptions.WithOptimizationLevel(optimizationLevel.Value);

            var projectInfo = ProjectInfo.Create(
                projectId, VersionStamp.Create(), "_", "_", Name,
                parseOptions: _parseOptions,
                compilationOptions: compilationOptions,
                metadataReferences: _metadataReferences,
                analyzerReferences: _defaultAnalyzerReferences
            );
            
            return new RoslynSession(
                SourceText.From(text, Encoding.UTF8),
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