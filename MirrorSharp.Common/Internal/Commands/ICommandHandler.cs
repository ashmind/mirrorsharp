using System;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace MirrorSharp.Internal.Commands {
    public interface ICommandHandler {
        [NotNull] IImmutableList<char> CommandIds { get; }
        [NotNull] Task ExecuteAsync(ArraySegment<byte> data, [NotNull] WorkSession session, [NotNull] ICommandResultSender sender, CancellationToken cancellationToken);
        bool CanChangeSession { get; }
    }
}
