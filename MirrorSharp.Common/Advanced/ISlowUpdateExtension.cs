using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace MirrorSharp.Advanced {
    [PublicAPI]
    public interface ISlowUpdateExtension {
        [NotNull, ItemCanBeNull] Task<object> PrepareAsync([NotNull] IWorkSession session, CancellationToken cancellationToken);
        void Write([NotNull] IFastJsonWriter writer, [CanBeNull] object prepared, CancellationToken cancellationToken);
    }
}
