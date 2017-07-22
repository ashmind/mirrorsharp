using System;
using System.Buffers;
using System.Collections.Immutable;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MirrorSharp.Advanced;
using MirrorSharp.Internal.Handlers;
using MirrorSharp.Internal.Results;

namespace MirrorSharp.Internal {
    internal class Connection : ICommandResultSender, IDisposable {
        public static int InputBufferSize => 4096;

        private readonly ArrayPool<byte> _bufferPool;
        private readonly WebSocket _socket;
        private readonly WorkSession _session;
        private readonly ImmutableArray<ICommandHandler> _handlers;
        private readonly byte[] _inputBuffer;

        private readonly FastUtf8JsonWriter _messageWriter;
        private readonly IConnectionOptions _options;

        public Connection(WebSocket socket, WorkSession session, ImmutableArray<ICommandHandler> handlers, ArrayPool<byte> bufferPool, IConnectionOptions options = null) {
            _socket = socket;
            _session = session;
            _handlers = handlers;
            _messageWriter = new FastUtf8JsonWriter(bufferPool);
            _options = options ?? new MirrorSharpOptions();
            _bufferPool = bufferPool;
            _inputBuffer = bufferPool.Rent(InputBufferSize);
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

        // ReSharper disable once HeapView.ClosureAllocation
        private async Task ReceiveAndProcessInternalAsync(CancellationToken cancellationToken) {
            var first = await _socket.ReceiveAsync(new ArraySegment<byte>(_inputBuffer), cancellationToken).ConfigureAwait(false);
            if (first.MessageType == WebSocketMessageType.Close) {
                await _socket.CloseAsync(first.CloseStatus ?? WebSocketCloseStatus.Empty, first.CloseStatusDescription, cancellationToken).ConfigureAwait(false);
                return;
            }
            
            if (first.MessageType == WebSocketMessageType.Binary) {
                await ReceiveToEndAsync(cancellationToken).ConfigureAwait(false);
                throw new FormatException("Expected text data (received binary).");
            }

            // it is important to record this conditionally on SelfDebug being enabled, otherwise
            // we lose no-allocation performance by allocating here
            var messageForDebug = _session.SelfDebug != null ? Encoding.UTF8.GetString(_inputBuffer, 0, first.Count) : null;
            _session.SelfDebug?.Log("before", messageForDebug, _session.CursorPosition, _session.GetText());

            var commandId = _inputBuffer[0];
            var handler = ResolveHandler(commandId);
            var last = first;
            await handler.ExecuteAsync(
                new AsyncData(
                    new ArraySegment<byte>(_inputBuffer, 1, first.Count - 1),
                    !first.EndOfMessage,
                    // Can we avoid this allocation?
                    async () => {
                        if (last.EndOfMessage)
                            return null;
                        last = await _socket.ReceiveAsync(new ArraySegment<byte>(_inputBuffer), cancellationToken).ConfigureAwait(false);
                        return new ArraySegment<byte>(_inputBuffer, 0, last.Count);
                    }
                ),
                _session, this, cancellationToken
            ).ConfigureAwait(false);

            if (!last.EndOfMessage) {
                await ReceiveToEndAsync(cancellationToken).ConfigureAwait(false);
                // ReSharper disable once HeapView.BoxingAllocation
                throw new InvalidOperationException($"Received message has unread data after command '{(char)commandId}'.");
            }

            _session.SelfDebug?.Log("after", messageForDebug, _session.CursorPosition, _session.GetText());
        }

        private async Task ReceiveToEndAsync(CancellationToken cancellationToken) {
            while (!(await _socket.ReceiveAsync(new ArraySegment<byte>(_inputBuffer), cancellationToken).ConfigureAwait(false)).EndOfMessage) {
            }
        }

        private ICommandHandler ResolveHandler(byte commandId) {
            var handlerIndex = commandId - (byte)'A';
            if (handlerIndex < 0 || handlerIndex > _handlers.Length - 1) {
                // ReSharper disable once HeapView.BoxingAllocation
                throw new FormatException($"Invalid command: '{(char)commandId}'.");
            }

            var handler = _handlers[handlerIndex];
            if (handler == null) {
                // ReSharper disable once HeapView.BoxingAllocation
                throw new FormatException($"Unknown command: '{(char)commandId}'.");
            }
            return handler;
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
            _bufferPool.Return(_inputBuffer);
            _messageWriter.Dispose();
            _session.Dispose();
        }

        IFastJsonWriter ICommandResultSender.StartJsonMessage(string messageTypeName) => StartJsonMessage(messageTypeName);
        Task ICommandResultSender.SendJsonMessageAsync(CancellationToken cancellationToken) => SendJsonMessageAsync(cancellationToken);
    }
}
