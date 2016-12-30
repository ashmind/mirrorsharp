using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using MirrorSharp.Internal.Results;

namespace MirrorSharp.Internal.Handlers.Shared {
    internal interface ICompletionSupport {
        [NotNull] Task ApplyTypedCharAsync(char @char, [NotNull] WorkSession session, [NotNull] ICommandResultSender sender, CancellationToken cancellationToken);
        [NotNull] Task ApplyReplacedTextAsync(string reason, [NotNull] ITypedCharEffects typedCharEffects, [NotNull] WorkSession session, ICommandResultSender sender, CancellationToken cancellationToken);
        [NotNull] Task SelectCompletionAsync(int selectedIndex, [NotNull] WorkSession session, [NotNull] ICommandResultSender sender, CancellationToken cancellationToken);
        [NotNull] Task CancelCompletionAsync([NotNull] WorkSession session, [NotNull] ICommandResultSender sender, CancellationToken cancellationToken);
        [NotNull] Task ForceCompletionAsync([NotNull] WorkSession session, [NotNull] ICommandResultSender sender, CancellationToken cancellationToken);
    }
}