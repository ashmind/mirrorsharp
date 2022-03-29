using System.Composition;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using MirrorSharp.Internal.RoslynInterfaces;

namespace MirrorSharp.Internal.Roslyn41 {
    [Shared]
    [Export(typeof(IWorkspaceAnalyzerOptionsInternals))]
    internal class WorkspaceAnalyzerOptionsInternals : IWorkspaceAnalyzerOptionsInternals {
        public AnalyzerOptions New(AnalyzerOptions options, Solution solution) {
            return new WorkspaceAnalyzerOptions(options, solution);
        }
    }
}
