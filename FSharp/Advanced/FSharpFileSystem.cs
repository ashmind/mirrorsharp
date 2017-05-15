using System.IO;
using JetBrains.Annotations;
using MirrorSharp.FSharp.Internal;

namespace MirrorSharp.FSharp.Advanced {
    [PublicAPI]
    public static class FSharpFileSystem {
        [NotNull] public static FSharpVirtualFile RegisterVirtualFile([NotNull] MemoryStream stream) => CustomFileSystem.Instance.RegisterVirtualFile(stream);
    }
}
