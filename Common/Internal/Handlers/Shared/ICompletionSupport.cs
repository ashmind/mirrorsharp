using System.Threading;
using System.Threading.Tasks;
using MirrorSharp.Internal.Results;

namespace MirrorSharp.Internal.Handlers.Shared {
    internal interface ICompletionSupport {
        Task ApplyTypedCharAsync(char @char, WorkSession session, ICommandResultSender sender, CancellationToken cancellationToken);
        Task ApplyReplacedTextAsync(string reason, ITypedCharEffects typedCharEffects, WorkSession session, ICommandResultSender sender, CancellationToken cancellationToken);
        Task SendItemInfoAsync(int selectedIndex, WorkSession session, ICommandResultSender sender, CancellationToken cancellationToken);
        Task SelectCompletionAsync(int selectedIndex, WorkSession session, ICommandResultSender sender, CancellationToken cancellationToken);
        Task CancelCompletionAsync(WorkSession session, ICommandResultSender sender, CancellationToken cancellationToken);
        Task ForceCompletionAsync(WorkSession session, ICommandResultSender sender, CancellationToken cancellationToken);
    }
}