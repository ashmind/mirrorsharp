using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;

namespace MirrorSharp.Advanced {
    [PublicAPI]
    public interface ISlowUpdateExtension {
        [NotNull, ItemCanBeNull] Task<object> ProcessAsync([NotNull] IWorkSession session, IList<Diagnostic> diagnostics, CancellationToken cancellationToken);
        void WriteResult([NotNull] IFastJsonWriter writer, [CanBeNull] object result);
    }
}
