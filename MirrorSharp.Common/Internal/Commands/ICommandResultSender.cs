using System.Threading;
using System.Threading.Tasks;

namespace MirrorSharp.Internal.Commands {
    public interface ICommandResultSender {
        FastJsonWriter StartJsonMessage(string messageTypeName);
        Task SendJsonMessageAsync(CancellationToken cancellationToken);
    }
}