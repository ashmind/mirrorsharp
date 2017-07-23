using System.IO;
using JetBrains.Annotations;

namespace MirrorSharp.FSharp.Internal {
    internal interface IVirtualFileInternal {
        [NotNull] string Name { get; }
        bool Exists { get; set; }
        [NotNull] Stream GetStream();
        [NotNull] byte[] ReadAllBytes();
    }
}
