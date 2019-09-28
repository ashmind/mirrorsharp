using System.Threading;
using System.Threading.Tasks;
using MirrorSharp.Internal.Results;

namespace MirrorSharp.Internal.Handlers {
    internal interface ICommandHandler {
        char CommandId { get; }
        Task ExecuteAsync(AsyncData data, WorkSession session, ICommandResultSender sender, CancellationToken cancellationToken);
    }
}
