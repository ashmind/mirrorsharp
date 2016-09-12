using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net.WebSockets;
using System.Threading;

namespace MirrorSharp.Owin.Internal {
    using WebSocketSendAsync = Func<
        ArraySegment<byte> /* data */,
        int /* messageType */,
        bool /* endOfMessage */,
        CancellationToken /* cancel */,
        Task
    >;

    using WebSocketReceiveAsync = Func<
        ArraySegment<byte> /* data */,
        CancellationToken /* cancel */,
        Task<
            Tuple<
                int /* messageType */,
                bool /* endOfMessage */,
                int /* count */
            >
        >
    >;
    
    using WebSocketCloseAsync = Func<
        int /* closeStatus */,
        string /* closeDescription */,
        CancellationToken /* cancel */,
        Task
    >;

    internal class OwinWebSocket : WebSocket {
        private readonly IDictionary<string, object> _environment;
        private readonly WebSocketSendAsync _sendAsync;
        private readonly WebSocketReceiveAsync _receiveAsync;
        private readonly WebSocketCloseAsync _closeAsync;
        private readonly WebSocketState _state;

        public OwinWebSocket(IDictionary<string, object> environment) {
            _environment = environment;
            _state = WebSocketState.Open;
            _sendAsync = (WebSocketSendAsync)environment["websocket.SendAsync"];
            _receiveAsync = (WebSocketReceiveAsync)environment["websocket.ReceiveAsync"];
            _closeAsync = (WebSocketCloseAsync)environment["websocket.CloseAsync"];
        }

        public override WebSocketCloseStatus? CloseStatus 
            => (WebSocketCloseStatus?)(int?)_environment.GetValueOrDefault("websocket.ClientCloseStatus");

        public override string CloseStatusDescription
            => (string)_environment.GetValueOrDefault("websocket.ClientCloseDescription");

        public override WebSocketState State => _state;

        public override string SubProtocol
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override async Task<WebSocketReceiveResult> ReceiveAsync(ArraySegment<byte> buffer, CancellationToken cancellationToken) {
            var tuple = await _receiveAsync(buffer, cancellationToken).ConfigureAwait(false);
            return new WebSocketReceiveResult(
                tuple.Item3, MapMessageTypeToEnum(tuple.Item1), tuple.Item2,
                CloseStatus, CloseStatusDescription
            );
        }

        public override Task SendAsync(ArraySegment<byte> buffer, WebSocketMessageType messageType, bool endOfMessage, CancellationToken cancellationToken) {
            return _sendAsync(buffer, MapMessageTypeFromEnum(messageType), endOfMessage, cancellationToken);
        }

        public override void Abort() {
            throw new NotImplementedException();
        }

        public override Task CloseAsync(WebSocketCloseStatus closeStatus, string statusDescription, CancellationToken cancellationToken) {
            throw new NotImplementedException();
        }

        public override Task CloseOutputAsync(WebSocketCloseStatus closeStatus, string statusDescription, CancellationToken cancellationToken) {
            return _closeAsync((int)closeStatus, statusDescription, cancellationToken);
        }

        private int MapMessageTypeFromEnum(WebSocketMessageType messageType) {
            switch (messageType) {
                case WebSocketMessageType.Binary: return OwinWebSocketMessageType.Binary;
                case WebSocketMessageType.Text: return OwinWebSocketMessageType.Text;
                case WebSocketMessageType.Close: return OwinWebSocketMessageType.Close;
                default: throw new ArgumentException($"Unknown message type: {messageType}.", nameof(messageType));
            }
        }

        private WebSocketMessageType MapMessageTypeToEnum(int messageType) {
            switch (messageType) {
                case OwinWebSocketMessageType.Binary: return WebSocketMessageType.Binary;
                case OwinWebSocketMessageType.Text: return WebSocketMessageType.Text;
                case OwinWebSocketMessageType.Close: return WebSocketMessageType.Close;
                default: throw new ArgumentException($"Unknown message type: {messageType}.", nameof(messageType));
            }
        }

        public override void Dispose() {
        }
    }
}
