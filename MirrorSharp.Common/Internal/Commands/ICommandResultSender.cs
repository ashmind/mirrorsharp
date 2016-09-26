using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace MirrorSharp.Internal.Commands {
    public interface ICommandResultSender {
        JsonWriter StartJsonMessage(string messageTypeName);
        Task SendJsonMessageAsync(CancellationToken cancellationToken);
    }
}