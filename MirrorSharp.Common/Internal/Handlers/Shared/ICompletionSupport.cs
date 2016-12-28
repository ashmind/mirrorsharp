using System.Threading;
using System.Threading.Tasks;
using MirrorSharp.Internal.Results;

namespace MirrorSharp.Internal.Handlers.Shared {
    internal interface ICompletionSupport {
        Task ApplyTypedCharAsync(char @char, WorkSession session, ICommandResultSender sender, CancellationToken cancellationToken);
        Task ApplyReplacedTextAsync(string reason, WorkSession session, ICommandResultSender sender, CancellationToken cancellationToken);
        Task ApplyCompletionSelectionAsync(int selectedIndex, WorkSession session, ICommandResultSender sender, CancellationToken cancellationToken);
        Task ApplyCompletionCancellationAsync(WorkSession session, ICommandResultSender sender, CancellationToken cancellationToken);
        Task ApplyCompletionForceAsync(WorkSession session, ICommandResultSender sender, CancellationToken cancellationToken);
    }
}