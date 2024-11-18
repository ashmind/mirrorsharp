using System;
using System.Collections.Generic;
using System.IO;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace MirrorSharp.Testing.Internal {
    internal class TestSocket : WebSocket {
        private readonly Queue<Queue<MemoryStream>> _dataToReceive = new();

        public override WebSocketCloseStatus? CloseStatus => throw new NotSupportedException();

        public override string CloseStatusDescription => throw new NotSupportedException();

        public override WebSocketState State => throw new NotSupportedException();

        public override string SubProtocol => throw new NotSupportedException();

        public override void Abort() {
            throw new NotSupportedException();
        }

        public override Task CloseAsync(WebSocketCloseStatus closeStatus, string statusDescription, CancellationToken cancellationToken) {
            throw new NotSupportedException();
        }

        public override Task CloseOutputAsync(WebSocketCloseStatus closeStatus, string statusDescription, CancellationToken cancellationToken) {
            throw new NotSupportedException();
        }

        public override void Dispose() {
            throw new NotSupportedException();
        }

        public override async Task<WebSocketReceiveResult> ReceiveAsync(ArraySegment<byte> buffer, CancellationToken cancellationToken) {
            var dataStream = _dataToReceive.Peek();
            if (dataStream == null)
                throw new Exception("No data was set up to be received");

            var count = await dataStream.ReadAsync(buffer.Array, buffer.Offset, buffer.Count);
            if (dataStream.Position == dataStream.Length)
                _dataToReceive.Dequeue();
            return new WebSocketReceiveResult(count, WebSocketMessageType.Text, dataStream.Position == dataStream.Length);
        }

        public override Task SendAsync(ArraySegment<byte> buffer, WebSocketMessageType messageType, bool endOfMessage, CancellationToken cancellationToken) {
            throw new NotSupportedException();
        }

        public void SetupToReceive(byte[] data) {
            _dataToReceive.Enqueue(new MemoryStream(data));
        }
    }
}
