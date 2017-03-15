using System.Threading.Tasks;

namespace MirrorSharp.Internal {
    internal static class TaskEx {
        public static Task CompletedTask { get; } = Task.FromResult((object)null);
    }
}
