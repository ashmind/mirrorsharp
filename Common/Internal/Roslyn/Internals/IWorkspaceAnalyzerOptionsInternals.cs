using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace MirrorSharp.Internal.Roslyn.Internals {
    internal interface IWorkspaceAnalyzerOptionsInternals {
        AnalyzerOptions New(AnalyzerOptions options, Solution solution);
    }
}
