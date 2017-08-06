using JetBrains.Annotations;
using MirrorSharp.Advanced;

namespace MirrorSharp.Internal {
    internal interface IConnectionOptions {
        bool IncludeExceptionDetails { get; set; }
        [CanBeNull] IExceptionLogger ExceptionLogger { get; set; }
    }
}