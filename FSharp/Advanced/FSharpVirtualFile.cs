using System;
using System.Collections.Concurrent;
using System.IO;

namespace MirrorSharp.FSharp.Advanced {
    /// <summary>Represents a virtual (in-memory) file within <see cref="FSharpFileSystem" />.</summary>
    public abstract class FSharpVirtualFile : IDisposable {
        private readonly ConcurrentDictionary<string, FSharpVirtualFile> _ownerCollection;

        private protected FSharpVirtualFile(
            string path,
            ConcurrentDictionary<string, FSharpVirtualFile> ownerCollection
        ) {
            Path = path;
            _ownerCollection = ownerCollection;
        }

        /// <summary>Gets the path of the virtual file (generated, unique).</summary>
        public string Path { get; }

        internal abstract MemoryStream GetStream();

        internal DateTime LastWriteTime { get; set; }

        /// <summary>Deregisters the file from the <see cref="FSharpFileSystem" />.</summary>
        public void Dispose() {
            _ownerCollection.TryRemove(Path, out var _);
        }
    }

    internal class FSharpVirtualFile<TGetStreamContext> : FSharpVirtualFile, IDisposable {
        private readonly TGetStreamContext _getStreamContext;
        private readonly Func<TGetStreamContext, MemoryStream> _getStream;

        public FSharpVirtualFile(
            string path,
            Func<TGetStreamContext, MemoryStream> getStream,
            TGetStreamContext getStreamContext,
            ConcurrentDictionary<string, FSharpVirtualFile> ownerCollection
        ) : base(path, ownerCollection) {
            _getStream = getStream;
            _getStreamContext = getStreamContext;
        }

        internal override MemoryStream GetStream() => _getStream(_getStreamContext);
    }
}