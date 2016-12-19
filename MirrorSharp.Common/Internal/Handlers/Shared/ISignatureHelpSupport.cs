using System.Threading;
using System.Threading.Tasks;
using MirrorSharp.Internal.Results;

namespace MirrorSharp.Internal.Handlers.Shared {
    public interface ISignatureHelpSupport {
        Task ApplyCursorPositionChangeAsync(WorkSession session, ICommandResultSender sender, CancellationToken cancellationToken);
        Task ApplyTypedCharAsync(char @char, WorkSession session, ICommandResultSender sender, CancellationToken cancellationToken);
    }
}