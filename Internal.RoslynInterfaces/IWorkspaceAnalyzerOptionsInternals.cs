using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace MirrorSharp.Internal.RoslynInterfaces {
    internal interface IWorkspaceAnalyzerOptionsInternals {
        AnalyzerOptions New(AnalyzerOptions options, Solution solution);
    }
}
