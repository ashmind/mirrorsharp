using MirrorSharp.Advanced.EarlyAccess;

namespace MirrorSharp.Internal.Roslyn {
    internal class RoslynLanguageDependencies {
        public RoslynLanguageDependencies(IRoslynGuard? guard) {
            Guard = guard;
        }

        public IRoslynGuard? Guard { get; }
    }
}
