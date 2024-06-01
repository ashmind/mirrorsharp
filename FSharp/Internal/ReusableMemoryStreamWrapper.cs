using System;
using System.IO;
using System.Threading;

namespace MirrorSharp.FSharp.Internal {
    internal class ReusableMemoryStreamWrapper : Stream {
        private readonly MemoryStream _stream;
        private readonly string _name;
        private int _inUse = 0;

        public ReusableMemoryStreamWrapper(MemoryStream stream, string name) {
            _stream = stream;
            _name = name;
        }

        internal Stream InnerStream => _stream;
        public override void Flush() => _stream.Flush();
        public override long Seek(long offset, SeekOrigin origin) => _stream.Seek(offset, origin);
        public override void SetLength(long value) => _stream.SetLength(value);
        public override int Read(byte[] buffer, int offset, int count) => _stream.Read(buffer, offset, count);
        public override void Write(byte[] buffer, int offset, int count) => _stream.Write(buffer, offset, count);
        public override bool CanRead => _stream.CanRead;
        public override bool CanSeek => _stream.CanSeek;
        public override bool CanWrite => _stream.CanWrite;
        public override long Length => _stream.Length;

        public override long Position {
            get => _stream.Position;
            set => _stream.Position = value;
        }

        public override void Close() {
            Flush();
            Position = 0;
            _inUse = 0;
        }

        internal ReusableMemoryStreamWrapper Reuse() {
            if (Interlocked.CompareExchange(ref _inUse, 1, 0) == 1)
                throw new InvalidOperationException($"Stream {_name} is currently in use, parallel access is not supported.");
            return this;
        }
    }
}
