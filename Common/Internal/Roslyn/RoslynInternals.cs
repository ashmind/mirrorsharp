using System;
using System.Composition.Hosting;
using System.IO;
using System.Reflection;
using MirrorSharp.Internal.RoslynInterfaces;

namespace MirrorSharp.Internal.Roslyn {
    internal class RoslynInternals {
        public RoslynInternals(
            ICodeActionInternals codeAction,
            IWorkspaceAnalyzerOptionsInternals workspaceAnalyzerOptions,
            ISignatureHelpProviderWrapperResolver singatureHelpProviderResolver
        ) {
            CodeAction = codeAction;
            WorkspaceAnalyzerOptions = workspaceAnalyzerOptions;
            SingatureHelpProviderResolver = singatureHelpProviderResolver;
        }

        public ICodeActionInternals CodeAction { get; }
        public IWorkspaceAnalyzerOptionsInternals WorkspaceAnalyzerOptions { get; }
        public ISignatureHelpProviderWrapperResolver SingatureHelpProviderResolver { get; }

        public static RoslynInternals Get(CompositionHost compositionHost) {
            Argument.NotNull(nameof(compositionHost), compositionHost);
            return new RoslynInternals(
                compositionHost.GetExport<ICodeActionInternals>(),
                compositionHost.GetExport<IWorkspaceAnalyzerOptionsInternals>(),
                compositionHost.GetExport<ISignatureHelpProviderWrapperResolver>()
            );
        }

        public static Assembly LoadInternalsAssemblySlow() {
            var roslynVersion = RoslynAssemblies.MicrosoftCodeAnalysis.GetName().Version;

            var assemblyName = roslynVersion switch {
                { Major: > 4 } => "MirrorSharp.Internal.Roslyn42",
                { Major: 4, Minor: >= 2 } => "MirrorSharp.Internal.Roslyn42",
                { Major: 4, Minor: 1 } => "MirrorSharp.Internal.Roslyn41",
                { Major: 4 } => "MirrorSharp.Internal.Roslyn36",
                { Major: 3, Minor: >= 6 } => "MirrorSharp.Internal.Roslyn36",
                { Major: 3, Minor: >= 3 } => "MirrorSharp.Internal.Roslyn33",
                _ => throw new NotSupportedException()
            };

            var basePath = AppDomain.CurrentDomain.BaseDirectory!;
            if (AppDomain.CurrentDomain.RelativeSearchPath is { } relativePath)
                basePath = Path.Combine(basePath, relativePath);

            var assemblyPath = Path.Combine(basePath, assemblyName + ".dll");
            return Assembly.LoadFrom(assemblyPath);
        }
    }
}
