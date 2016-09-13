using System.Threading.Tasks;

namespace MirrorSharp.Internal {
    public interface IAsyncDisposable {
        Task DisposeAsync();
    }
}