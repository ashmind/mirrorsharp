using MirrorSharp.Advanced;

namespace MirrorSharp.Internal {
    internal class ImmutableExtensionServices {
        public ImmutableExtensionServices(
            ISetOptionsFromClientExtension? setOptionsFromClient,
            ISlowUpdateExtension? slowUpdate,
            IExceptionLogger? exceptionLogger
        ) {
            SetOptionsFromClient = setOptionsFromClient;
            SlowUpdate = slowUpdate;
            ExceptionLogger = exceptionLogger;
        }

        public ISetOptionsFromClientExtension? SetOptionsFromClient { get; }
        public ISlowUpdateExtension? SlowUpdate { get; }
        public IExceptionLogger? ExceptionLogger { get; }
    }
}
