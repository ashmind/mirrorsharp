using System;
using System.Collections.Concurrent;
using System.IO;

namespace MirrorSharp.FSharp.Advanced {
    public class FSharpVirtualFile : IDisposable {
        private readonly ConcurrentDictionary<string, FSharpVirtualFile> _ownerCollection;

        public FSharpVirtualFile(string name, MemoryStream stream, ConcurrentDictionary<string, FSharpVirtualFile> ownerCollection) {
            Name = name;
            Stream = stream;
            _ownerCollection = ownerCollection;
        }

        public string Name { get; }
        public MemoryStream Stream { get; }

        public void Dispose() {
            _ownerCollection.TryRemove(Name, out FSharpVirtualFile _);
        }
    }
}