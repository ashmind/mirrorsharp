using System;
using System.Threading.Tasks;

namespace MirrorSharp.Internal {
    internal static class TaskEx {
        [Obsolete("Use Task.CompletedTask instead.")]
        public static Task CompletedTask { get; } = Task.FromResult((object)null);
    }
}
