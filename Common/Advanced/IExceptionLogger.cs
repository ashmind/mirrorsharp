using System;
using JetBrains.Annotations;

namespace MirrorSharp.Advanced {
    /// <summary>Provides a way to log unhandled exceptions.</summary>
    public interface IExceptionLogger {
        /// <summary>Logs a given exception.</summary>
        /// <param name="exception">Exception to log.</param>
        /// <param name="session">Current <see cref="IWorkSession" /></param>
        /// <remarks>Implementations should avoid throwing exceptions from this method.</remarks>
        void LogException([NotNull] Exception exception, [NotNull] IWorkSession session);
    }
}
