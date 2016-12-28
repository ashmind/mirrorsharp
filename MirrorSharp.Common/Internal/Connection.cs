using System;
using System.Buffers;
using System.Collections.Immutable;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MirrorSharp.Internal.Handlers;
using MirrorSharp.Internal.Results;

namespace MirrorSharp.Internal {
    internal class Connection : ICommandResultSender, IDisposable {
        private readonly WebSocket _socket;
        private readonly WorkSession _session;
        private readonly ImmutableArray<ICommandHandler> _handlers;
        private readonly byte[] _inputBuffer = new byte[4096];

        private readonly FastUtf8JsonWriter _messageWriter;
        private readonly IConnectionOptions _options;

        public Connection(WebSocket socket, WorkSession session, ImmutableArray<ICommandHandler> handlers, IConnectionOptions options = null) {
            _socket = socket;
            _session = session;
            _handlers = handlers;
            _messageWriter = new FastUtf8JsonWriter(ArrayPool<byte>.Shared);
            _options = options ?? new MirrorSharpOptions();
        }

        public bool IsConnected => _socket.State == WebSocketState.Open;

        public async Task ReceiveAndProcessAsync(CancellationToken cancellationToken) {
            try {
                await ReceiveAndProcessInternalAsync(cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex) {
                try {
                    var error = _options.IncludeExceptionDetails ? ex.ToString() : "A server error has occurred.";
                    await SendErrorAsync(error, cancellationToken).ConfigureAwait(false);
                }
                catch (Exception sendException) {
                    throw new AggregateException(ex, sendException);
                }
                throw;
            }
        }

        private async Task ReceiveAndProcessInternalAsync(CancellationToken cancellationToken) {
            var received = await _socket.ReceiveAsync(new ArraySegment<byte>(_inputBuffer), cancellationToken).ConfigureAwait(false);
            if (received.MessageType == WebSocketMessageType.Binary)
                throw new FormatException("Expected text data (received binary).");

            if (received.MessageType == WebSocketMessageType.Close) {
                await _socket.CloseAsync(received.CloseStatus ?? WebSocketCloseStatus.Empty, received.CloseStatusDescription, cancellationToken).ConfigureAwait(false);
                return;
            }

            // it is important to record this conditionally on SelfDebug being enabled, otherwise
            // we lose no-allocation performance by allocating here
            var messageForDebug = _session.SelfDebug != null ? Encoding.UTF8.GetString(_inputBuffer, 0, received.Count) : null;
            _session.SelfDebug?.Log("before", messageForDebug, _session.CursorPosition, _session.SourceText);

            var data = new ArraySegment<byte>(_inputBuffer, 0, received.Count);
            var commandId = data.Array[data.Offset];
            var handler = ResolveHandler(commandId);
            await handler.ExecuteAsync(Shift(data), _session, this, cancellationToken).ConfigureAwait(false);

            _session.SelfDebug?.Log("after", messageForDebug, _session.CursorPosition, _session.SourceText);
        }

        private ICommandHandler ResolveHandler(byte commandId) {
            var handlerIndex = commandId - (byte)'A';
            if (handlerIndex < 0 || handlerIndex > _handlers.Length - 1)
                throw new FormatException($"Invalid command: '{(char)commandId}'.");

            var handler = _handlers[handlerIndex];
            if (handler == null)
                throw new FormatException($"Unknown command: '{(char)commandId}'.");
            return handler;
        }

        private ArraySegment<byte> Shift(ArraySegment<byte> data) {
            return new ArraySegment<byte>(data.Array, data.Offset + 1, data.Count - 1);
        }

        private Task SendErrorAsync(string message, CancellationToken cancellationToken) {
            var writer = StartJsonMessage("error");
            writer.WriteProperty("message", message);
            return SendJsonMessageAsync(cancellationToken);
        }

        private FastUtf8JsonWriter StartJsonMessage(string messageTypeName) {
            _messageWriter.Reset();
            _messageWriter.WriteStartObject();
            _messageWriter.WriteProperty("type", messageTypeName);
            return _messageWriter;
        }

        private Task SendJsonMessageAsync(CancellationToken cancellationToken) {
            _messageWriter.WriteEndObject();
            return _socket.SendAsync(
                _messageWriter.WrittenSegment,
                WebSocketMessageType.Text, true, cancellationToken
            );
        }

        public void Dispose() {
            _messageWriter.Dispose();
            _session.Dispose();
        }

        IFastJsonWriterInternal ICommandResultSender.StartJsonMessage(string messageTypeName) => StartJsonMessage(messageTypeName);
        Task ICommandResultSender.SendJsonMessageAsync(CancellationToken cancellationToken) => SendJsonMessageAsync(cancellationToken);
    }
}
