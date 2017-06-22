using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;

namespace MirrorSharp.Advanced {
    /// <summary>An interface used to implement periodic custom processing.</summary>
    [PublicAPI]
    public interface ISlowUpdateExtension {
        /// <summary>Method called by MirrorSharp periodically (e.g. each 500ms), if there were any changes.</summary>
        /// <param name="session">Current <see cref="IWorkSession" />.</param>
        /// <param name="diagnostics">Current diagnostics. <see cref="ProcessAsync" /> can add extra diagnosics if needed.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that MirrorSharp can use to cancel processing.</param>
        /// <returns>Any object; result will be passed to <see cref="WriteResult" />.</returns>
        /// <remarks>If the return value implements <see cref="IDisposable" />, it will be automatically disposed after <see cref="WriteResult" /> call.</remarks>
        [NotNull, ItemCanBeNull] Task<object> ProcessAsync([NotNull] IWorkSession session, IList<Diagnostic> diagnostics, CancellationToken cancellationToken);

        /// <summary>Called after <see cref="ProcessAsync" />; writes its result to the client if required.</summary>
        /// <param name="writer"><see cref="IFastJsonWriter"/> used to write the result.</param>
        /// <param name="result">Result returned by <see cref="ProcessAsync" />.</param>
        /// <param name="session">Current <see cref="IWorkSession" />.</param>
        void WriteResult([NotNull] IFastJsonWriter writer, [CanBeNull] object result, [NotNull] IWorkSession session);
    }
}
