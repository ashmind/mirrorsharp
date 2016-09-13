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
        private readonly TaskCompletionSource<object> _abortTaskSource;
        private WebSocketState _state;
        private WebSocketCloseStatus? _closeStatus;
        private string _closeDescription;

        public OwinWebSocket(IDictionary<string, object> environment) {
            _environment = environment;
            _state = WebSocketState.Open;
            _sendAsync = (WebSocketSendAsync)environment["websocket.SendAsync"];
            _receiveAsync = (WebSocketReceiveAsync)environment["websocket.ReceiveAsync"];
            _closeAsync = (WebSocketCloseAsync)environment["websocket.CloseAsync"];

            var callCancelledToken = (CancellationToken)environment["websocket.CallCancelled"];
            callCancelledToken.Register(Abort);

            _abortTaskSource = new TaskCompletionSource<object>();
        }

        public override WebSocketCloseStatus? CloseStatus => _closeStatus;
        public override string CloseStatusDescription => _closeDescription;
        public override WebSocketState State => _state;

        public Task AbortedTask => _abortTaskSource.Task;

        public override string SubProtocol
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override async Task<WebSocketReceiveResult> ReceiveAsync(ArraySegment<byte> buffer, CancellationToken cancellationToken) {
            if (_state != WebSocketState.Open && _state != WebSocketState.CloseSent)
                throw new InvalidOperationException($"WebSocket state is {_state}: cannot receieve data.");

            try {
                var tuple = await _receiveAsync(buffer, cancellationToken).ConfigureAwait(false);
                var messageType = MapMessageTypeToEnum(tuple.Item1);
                if (_state == WebSocketState.CloseSent && messageType != WebSocketMessageType.Close)
                    throw new WebSocketException(WebSocketError.InvalidMessageType);

                if (messageType == WebSocketMessageType.Close) {
                    _state = (_state == WebSocketState.CloseSent) ? WebSocketState.Closed : WebSocketState.CloseReceived;
                    _closeStatus = (WebSocketCloseStatus?)(int?)_environment.GetValueOrDefault("websocket.ClientCloseStatus");
                    _closeDescription = (string)_environment.GetValueOrDefault("websocket.ClientCloseDescription");
                    return new WebSocketReceiveResult(
                        tuple.Item3, messageType, tuple.Item2,
                        _closeStatus, _closeDescription
                    );
                }

                return new WebSocketReceiveResult(tuple.Item3, messageType, tuple.Item2);
            }
            catch (WebSocketException) {
                Abort();
                throw;
            }
        }

        public override async Task SendAsync(ArraySegment<byte> buffer, WebSocketMessageType messageType, bool endOfMessage, CancellationToken cancellationToken) {
            if (_state != WebSocketState.Open && _state != WebSocketState.CloseReceived)
                throw new InvalidOperationException($"WebSocket state is {_state}: cannot send data.");

            try {
                await _sendAsync(buffer, MapMessageTypeFromEnum(messageType), endOfMessage, cancellationToken)
                    .ConfigureAwait(false);
            }
            catch (Exception ex) when (messageType == WebSocketMessageType.Close || ex is WebSocketException) {
                Abort();
                throw;
            }

            if (messageType == WebSocketMessageType.Close)
                _state = (_state == WebSocketState.CloseReceived) ? WebSocketState.Closed : WebSocketState.CloseSent;
        }

        public override void Abort() {
            if (_state == WebSocketState.Aborted)
                return;

            _state = WebSocketState.Aborted;
            _abortTaskSource.SetResult(null);
        }

        public override async Task CloseAsync(WebSocketCloseStatus closeStatus, string statusDescription, CancellationToken cancellationToken) {
            try {
                await CloseOutputAsync(closeStatus, statusDescription, cancellationToken).ConfigureAwait(false);
                await ReceiveAsync(new ArraySegment<byte>(new byte[123]), cancellationToken).ConfigureAwait(false);
            }
            catch (Exception) {
                Abort();
                throw;
            }
        }

        public override async Task CloseOutputAsync(WebSocketCloseStatus closeStatus, string statusDescription, CancellationToken cancellationToken) {
            if (_state != WebSocketState.Open)
                throw new InvalidOperationException($"WebSocket state is {_state}: cannot close.");

            try {
                await _closeAsync((int)closeStatus, statusDescription, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception) {
                Abort();
                throw;
            }
            _closeStatus = closeStatus;
            _closeDescription = statusDescription;
            _state = WebSocketState.CloseSent;
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
