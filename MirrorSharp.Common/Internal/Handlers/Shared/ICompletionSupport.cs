using System.Threading;
using System.Threading.Tasks;
using MirrorSharp.Internal.Results;

namespace MirrorSharp.Internal.Handlers.Shared {
    public interface ICompletionSupport {
        Task ApplyTypedCharAsync(char @char, WorkSession session, ICommandResultSender sender, CancellationToken cancellationToken);
        Task ApplyCompletionSelectionAsync(int selectedIndex, WorkSession session, ICommandResultSender sender, CancellationToken cancellationToken);
        Task ApplyCompletionStateChangeAsync(CompletionStateChange change, WorkSession session, ICommandResultSender sender, CancellationToken cancellationToken);
    }
}