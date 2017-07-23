using System;
using System.Collections.Concurrent;
using System.IO;
using JetBrains.Annotations;
using MirrorSharp.FSharp.Internal;

namespace MirrorSharp.FSharp.Advanced {
    /// <summary>Represents a virtual (in-memory) file within <see cref="FSharpFileSystem" />.</summary>
    [PublicAPI]
    public class FSharpVirtualFile : IVirtualFileInternal, IDisposable {
        private readonly ConcurrentDictionary<string, IVirtualFileInternal> _ownerCollection;
        private NonDisposingStreamWrapper _streamWrapper;

        internal FSharpVirtualFile(string name, MemoryStream stream, bool exists, ConcurrentDictionary<string, IVirtualFileInternal> ownerCollection) {
            Name = name;
            Exists = exists;
            Stream = stream;
            _ownerCollection = ownerCollection;
        }

        /// <summary>Gets the name of the virtual file (generated, unique).</summary>
        public string Name { get; }

        /// <summary>Gets the MemoryStream representing contents of the virtual file.</summary>
        public MemoryStream Stream { get; }

        /// <summary>Gets whether the virtual file is seen as existing in the file system.</summary>
        public bool Exists { get; internal set; }

        bool IVirtualFileInternal.Exists {
            get => Exists;
            set => Exists = value;
        }

        Stream IVirtualFileInternal.GetStream() {
            if (_streamWrapper == null)
                _streamWrapper = new NonDisposingStreamWrapper(Stream);
            return _streamWrapper;
        }

        byte[] IVirtualFileInternal.ReadAllBytes() {
            return Stream.ToArray();
        }

        /// <summary>Deregisters the file from the <see cref="FSharpFileSystem" />.</summary>
        public void Dispose() {
            _ownerCollection.TryRemove(Name, out IVirtualFileInternal _);
        }
    }
}