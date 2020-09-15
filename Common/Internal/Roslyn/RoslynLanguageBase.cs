using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition.Hosting;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Host.Mef;
using Microsoft.CodeAnalysis.Text;
using MirrorSharp.Internal.Abstraction;
using MirrorSharp.Internal.Reflection;

namespace MirrorSharp.Internal.Roslyn {
    internal abstract class RoslynLanguageBase : ILanguage {
        private readonly IRoslynLanguageOptions _options;
        private readonly MefHostServices _hostServices;
        private readonly ImmutableArray<ISignatureHelpProviderWrapper> _defaultSignatureHelpProviders;
        private readonly ImmutableDictionary<string, ImmutableArray<CodeFixProvider>> _codeFixProvidersIndexedByDiagnosticIds;
        private readonly ImmutableArray<DiagnosticAnalyzer> _analyzers;

        protected RoslynLanguageBase(
            string name,
            string featuresAssemblyName,
            string workspacesAssemblyName,
            IRoslynLanguageOptions options
        ) {
            Name = name;
            _hostServices = CreateHostServices(featuresAssemblyName, workspacesAssemblyName);

            _options = options;
            _codeFixProvidersIndexedByDiagnosticIds = CreateDefaultCodeFixProvidersSlow();
            _analyzers = ImmutableArray.CreateRange(
                _options.AnalyzerReferences.SelectMany(r => r.GetAnalyzers(Name))
            );
            _defaultSignatureHelpProviders = CreateDefaultSignatureHelpProvidersSlow();
        }

        private MefHostServices CreateHostServices(string featuresAssemblyName, string workspacesAssemblyName) {
            var types = new[] {
                RoslynAssemblies.MicrosoftCodeAnalysisWorkspaces,
                RoslynAssemblies.MicrosoftCodeAnalysisFeatures,
                Assembly.Load(new AssemblyName(featuresAssemblyName)),
                Assembly.Load(new AssemblyName(workspacesAssemblyName))
            }.SelectMany(a => a.DefinedTypes).Where(ShouldConsiderForHostServices);

            var configuration = new ContainerConfiguration().WithParts(types);
            return MefHostServices.Create(configuration.CreateContainer());
        }

        protected virtual bool ShouldConsiderForHostServices(Type type) => true;

        public string Name { get; }

        public ILanguageSessionInternal CreateSession(string text) {
            var projectId = ProjectId.CreateNewId();

            var projectInfo = ProjectInfo.Create(
                projectId, VersionStamp.Create(), "_", "_", Name,
                parseOptions: _options.ParseOptions,
                isSubmission: _options.IsScript,
                hostObjectType: _options.HostObjectType,
                compilationOptions: _options.CompilationOptions,
                metadataReferences: _options.MetadataReferences,
                analyzerReferences: _options.AnalyzerReferences
            );

            return new RoslynSession(
                SourceText.From(text, Encoding.UTF8),
                projectInfo,
                _hostServices,
                _analyzers,
                _codeFixProvidersIndexedByDiagnosticIds,
                _defaultSignatureHelpProviders
            );
        }

        private ImmutableArray<ISignatureHelpProviderWrapper> CreateDefaultSignatureHelpProvidersSlow() {
            return ImmutableArray.CreateRange(
                RoslynReflection.GetSignatureHelpProvidersSlow(_hostServices)
                    .Where(l => l.Metadata.Language == Name)
                    .Select(l => l.Value)
            );
        }

        private ImmutableDictionary<string, ImmutableArray<CodeFixProvider>> CreateDefaultCodeFixProvidersSlow() {
            var codeFixProviderTypes = _options
                .AnalyzerReferences
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
    }
}