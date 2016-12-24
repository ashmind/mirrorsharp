using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Host.Mef;
using MirrorSharp.Internal.Reflection;

namespace MirrorSharp.Internal.Languages {
    public class CSharpLanguage : ILanguage {
        public CSharpLanguage() {
            HostServices = MefHostServices.Create(MefHostServices.DefaultAssemblies.AddRange(new[] {
                Assembly.Load(new AssemblyName("Microsoft.CodeAnalysis.Features")),
                Assembly.Load(new AssemblyName("Microsoft.CodeAnalysis.CSharp.Features"))
            }));
            DefaultAssemblyReferences = ImmutableList.Create<MetadataReference>(
                MetadataReference.CreateFromFile(typeof(object).GetTypeInfo().Assembly.Location)
            );
            DefaultAnalyzerReferences = ImmutableList.Create<AnalyzerReference>(
                CreateAnalyzerReference("Microsoft.CodeAnalysis.CSharp.Features")
            );
            DefaultCodeFixProvidersIndexedByDiagnosticIds = CreateDefaultCodeFixProviders();
            DefaultAnalyzers = ImmutableArray.CreateRange(
                DefaultAnalyzerReferences.SelectMany(r => r.GetAnalyzers(Name))
            );
            SignatureHelpProviders = CreateDefaultSignatureHelpProviders();
        }

        public string Name => LanguageNames.CSharp;
        public ImmutableList<MetadataReference> DefaultAssemblyReferences { get; }
        public ImmutableList<AnalyzerReference> DefaultAnalyzerReferences { get; }
        public ImmutableArray<DiagnosticAnalyzer> DefaultAnalyzers { get; }
        public ImmutableDictionary<string, ImmutableArray<CodeFixProvider>> DefaultCodeFixProvidersIndexedByDiagnosticIds { get; }
        internal ImmutableArray<ISignatureHelpProviderWrapper> SignatureHelpProviders { get; }
        ImmutableArray <ISignatureHelpProviderWrapper> ILanguage.DefaultSignatureHelpProviders => SignatureHelpProviders;
        public MefHostServices HostServices { get; }

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
