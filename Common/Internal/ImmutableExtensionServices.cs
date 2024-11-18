using MirrorSharp.Advanced;
using MirrorSharp.Advanced.EarlyAccess;

namespace MirrorSharp.Internal {
    internal class ImmutableExtensionServices : ILanguageSessionExtensions {
        public ImmutableExtensionServices(
            ISetOptionsFromClientExtension? setOptionsFromClient,
            ISlowUpdateExtension? slowUpdate,
            IRoslynSourceTextGuard? roslynSourceTextGuard,
            IRoslynCompilationGuard? roslynCompilationGuard,
            IConnectionSendViewer? connectionSendViewer,
            IExceptionLogger? exceptionLogger,
            IWorkSessionTracker? sessionTracker
        ) {
            SetOptionsFromClient = setOptionsFromClient;
            SlowUpdate = slowUpdate;
            RoslynSourceTextGuard = roslynSourceTextGuard;
            RoslynCompilationGuard = roslynCompilationGuard;
            ConnectionSendViewer = connectionSendViewer;
            ExceptionLogger = exceptionLogger;
            SessionTracker = sessionTracker;
        }

        public ISetOptionsFromClientExtension? SetOptionsFromClient { get; }
        public ISlowUpdateExtension? SlowUpdate { get; }
        public IRoslynSourceTextGuard? RoslynSourceTextGuard { get; }
        public IRoslynCompilationGuard? RoslynCompilationGuard { get; }
        public IConnectionSendViewer? ConnectionSendViewer { get; }
        public IExceptionLogger? ExceptionLogger { get; }
        public IWorkSessionTracker? SessionTracker { get; }
    }
}
