using System.Threading;
using System.Threading.Tasks;

namespace MirrorSharp.Internal.Results {
    public interface ICommandResultSender {
        FastUtf8JsonWriter StartJsonMessage(string messageTypeName);
        Task SendJsonMessageAsync(CancellationToken cancellationToken);
    }
}