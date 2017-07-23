using System;
using System.IO;
using System.Text;

namespace MirrorSharp.FSharp.Internal {
    internal class VirtualSourceFile : IVirtualFileInternal {
        private readonly Func<string> _getSourceText;

        public VirtualSourceFile(Func<string> getSourceText) {
            Name = "m" + Guid.NewGuid().ToString("N") + ".fs";
            Exists = true;
            _getSourceText = getSourceText;
        }

        public string Name { get; private set; }
        public bool Exists { get; set; }

        public Stream GetStream() {
            return new MemoryStream(ReadAllBytes());
        }

        public byte[] ReadAllBytes() {
            return Encoding.UTF8.GetBytes(_getSourceText());
        }
    }
}
