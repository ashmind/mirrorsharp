using MirrorSharp.Advanced.EarlyAccess;

namespace MirrorSharp.Internal.Roslyn {
    internal class RoslynLanguageDependencies {
        public RoslynLanguageDependencies(IRoslynCompilationGuard? guard) {
            Guard = guard;
        }

        public IRoslynCompilationGuard? Guard { get; }
    }
}
