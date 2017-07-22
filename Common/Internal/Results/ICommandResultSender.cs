using System.Threading;
using System.Threading.Tasks;
using MirrorSharp.Advanced;

namespace MirrorSharp.Internal.Results {
    internal interface ICommandResultSender {
        IFastJsonWriter StartJsonMessage(string messageTypeName);
        Task SendJsonMessageAsync(CancellationToken cancellationToken);
    }
}