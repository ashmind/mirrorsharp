using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using MirrorSharp.Internal.Results;

namespace MirrorSharp.Internal.Handlers.Shared {
    internal interface ISignatureHelpSupport {
        [NotNull] Task ApplyCursorPositionChangeAsync([NotNull] WorkSession session, [NotNull] ICommandResultSender sender, CancellationToken cancellationToken);
        [NotNull] Task ApplyTypedCharAsync(char @char, [NotNull] WorkSession session, [NotNull] ICommandResultSender sender, CancellationToken cancellationToken);
        [NotNull] Task ForceSignatureHelpAsync([NotNull] WorkSession session, [NotNull] ICommandResultSender sender, CancellationToken cancellationToken);
    }
}