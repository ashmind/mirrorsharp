using System.Composition.Hosting;
using MirrorSharp.Internal.Roslyn.Internals;

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
    }
}
