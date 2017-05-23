using System;
using System.Collections.Concurrent;
using System.IO;
using JetBrains.Annotations;

namespace MirrorSharp.FSharp.Advanced {
    /// <summary>Represents a virtual (in-memory) file within <see cref="FSharpFileSystem" />.</summary>
    [PublicAPI]
    public class FSharpVirtualFile : IDisposable {
        private readonly ConcurrentDictionary<string, FSharpVirtualFile> _ownerCollection;

        internal FSharpVirtualFile(string name, MemoryStream stream, ConcurrentDictionary<string, FSharpVirtualFile> ownerCollection) {
            Name = name;
            Stream = stream;
            _ownerCollection = ownerCollection;
        }

        /// <summary>Gets the name of the virtual file (generated, unique).</summary>
        public string Name { get; }

        /// <summary>Gets the MemoryStream representing contents of the virtual file.</summary>
        public MemoryStream Stream { get; }

        /// <summary>Deregisters the file from the <see cref="FSharpFileSystem" />.</summary>
        public void Dispose() {
            _ownerCollection.TryRemove(Name, out FSharpVirtualFile _);
        }
    }
}