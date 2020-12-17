using MirrorSharp.Advanced;
using MirrorSharp.Advanced.EarlyAccess;

namespace MirrorSharp.Internal {
    internal class ImmutableExtensionServices : ILanguageSessionExtensions {
        public ImmutableExtensionServices(
            ISetOptionsFromClientExtension? setOptionsFromClient,
            ISlowUpdateExtension? slowUpdate,
            IRoslynGuard? roslynGuard,
            IExceptionLogger? exceptionLogger
        ) {
            SetOptionsFromClient = setOptionsFromClient;
            SlowUpdate = slowUpdate;
            ExceptionLogger = exceptionLogger;
            RoslynGuard = roslynGuard;
        }

        public ISetOptionsFromClientExtension? SetOptionsFromClient { get; }
        public ISlowUpdateExtension? SlowUpdate { get; }
        public IExceptionLogger? ExceptionLogger { get; }
        public IRoslynGuard? RoslynGuard { get; }
    }
}
