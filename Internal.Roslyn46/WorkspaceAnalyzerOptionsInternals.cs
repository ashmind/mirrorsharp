using System.Composition;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using MirrorSharp.Internal.Roslyn.Internals;

namespace MirrorSharp.Internal.Roslyn46 {
    [Shared]
    [Export(typeof(IWorkspaceAnalyzerOptionsInternals))]
    internal class WorkspaceAnalyzerOptionsInternals : IWorkspaceAnalyzerOptionsInternals {
        public AnalyzerOptions New(AnalyzerOptions options, Project project) {
            Argument.NotNull(nameof(options), options);
            Argument.NotNull(nameof(project), project);
            return new WorkspaceAnalyzerOptions(
                options, IdeAnalyzerOptions.GetDefault(project.Services)
            );
        }
    }
}
