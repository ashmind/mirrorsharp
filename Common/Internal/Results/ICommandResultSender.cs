using System.Threading;
using System.Threading.Tasks;

namespace MirrorSharp.Internal.Results {
    internal interface ICommandResultSender {
        IFastJsonWriterInternal StartJsonMessage(string messageTypeName);
        Task SendJsonMessageAsync(CancellationToken cancellationToken);
    }
}