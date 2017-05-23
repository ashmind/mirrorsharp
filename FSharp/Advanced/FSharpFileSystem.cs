using System.IO;
using JetBrains.Annotations;
using MirrorSharp.FSharp.Internal;

namespace MirrorSharp.FSharp.Advanced {
    /// <summary>Represents a configuration endpoint for F# virtual filesystem.</summary>
    [PublicAPI]
    public static class FSharpFileSystem {
        /// <summary>Registers a new virtual file with unique name in the F# filesystem.</summary>
        /// <param name="stream"><see cref="MemoryStream" /> representing the content of the file.</param>
        /// <returns><see cref="FSharpVirtualFile" /> object that provides the unique name and allows deregistration.</returns>
        /// <remarks><see cref="FSharpVirtualFile.Dispose()" /> should be used to deregister the file.</remarks>
        [NotNull] public static FSharpVirtualFile RegisterVirtualFile([NotNull] MemoryStream stream) {
            Argument.NotNull(nameof(stream), stream);
            return CustomFileSystem.Instance.RegisterVirtualFile(stream);
        }
    }
}
