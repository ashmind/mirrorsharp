using System;
using System.Buffers;
using System.Collections.Immutable;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MirrorSharp.Advanced;
using MirrorSharp.Advanced.EarlyAccess;
using MirrorSharp.Internal.Handlers;
using MirrorSharp.Internal.Results;

namespace MirrorSharp.Internal {
    internal class Connection : IConnection, ICommandResultSender {
        public static int InputBufferSize => 4096;

        private readonly ArrayPool<byte> _inputBufferPool;
        private readonly IConnectionSendViewer? _sendViewer;
        private readonly WebSocket _socket;
        private readonly WorkSession _session;
        private readonly ImmutableArray<ICommandHandler> _handlers;
        private readonly byte[] _inputBuffer;

        private readonly ConnectionMessageWriter _messageWriter;
        private readonly IConnectionOptions? _options;
        private readonly IExceptionLogger? _exceptionLogger;

        public Connection(
            WebSocket socket,
            WorkSession session,
            ArrayPool<byte> inputBufferPool,
            ImmutableArray<ICommandHandler> handlers,
            ConnectionMessageWriter messageWriter,
            IConnectionSendViewer? sendViewer,
            IExceptionLogger? exceptionLogger,
            IConnectionOptions? options
        ) {
            _socket = socket;
            _session = session;
            _handlers = handlers;
            _messageWriter = messageWriter;
            _options = options;
            _sendViewer = sendViewer;
            _exceptionLogger = exceptionLogger;
            _inputBufferPool = inputBufferPool;
            _inputBuffer = inputBufferPool.Rent(InputBufferSize);
        }

        public bool IsConnected => _socket.State == WebSocketState.Open;

        public async Task ReceiveAndProcessAsync(CancellationToken cancellationToken) {
            try {
                await ReceiveAndProcessInternalAsync(cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex) {
                var exception = ex;
                try {
                    try {
                        _exceptionLogger?.LogException(exception, _session);
                    }
                    catch (Exception logException) {
                        exception = new AggregateException(exception, logException);
                    }
                    var error = (_options?.IncludeExceptionDetails ?? false) ? exception.ToString() : "A server error has occurred.";
                    await SendErrorAsync(error, cancellationToken).ConfigureAwait(false);
                }
                catch (Exception sendException) {
                    throw new AggregateException(ex, sendException).Flatten();
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
                    _inputBuffer.AsMemory(1, first.Count - 1),
                    !first.EndOfMessage,
                    // Can we avoid this allocation?
                    async () => {
                        if (last.EndOfMessage)
                            return null;
                        last = await _socket.ReceiveAsync(new ArraySegment<byte>(_inputBuffer), cancellationToken).ConfigureAwait(false);
                        return _inputBuffer.AsMemory(0, last.Count);
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
            _messageWriter.WriteErrorStart(message);
            return SendJsonMessageAsync(cancellationToken);
        }

        private Task SendJsonMessageAsync(CancellationToken cancellationToken) {
            _messageWriter.WriteMessageEnd();

            var viewTask = _sendViewer?.ViewDuringSendAsync(
                _messageWriter.CurrentMessageTypeName!,
                _messageWriter.WrittenSegment,
                _session,
                cancellationToken
            );
            var sendTask = _socket.SendAsync(
                _messageWriter.WrittenSegment,
                WebSocketMessageType.Text, true, cancellationToken
            );

            if (viewTask is { IsCompleted: false })
                return WhenAll(viewTask, sendTask);

            return sendTask;
        }

        private async Task WhenAll(Task first, Task second) {
            await first;
            await second;
        }

        public void Dispose() {
            try {
                try {
                    _inputBufferPool.Return(_inputBuffer);
                }
                finally {
                    _messageWriter.Dispose();
                }
            }
            finally {
                _session.Dispose();
            }
        }

        IFastJsonWriter ICommandResultSender.StartJsonMessage(string messageTypeName) {
            _messageWriter.WriteMessageStart(messageTypeName);
            return _messageWriter.JsonWriter;
        }

        Task ICommandResultSender.SendJsonMessageAsync(CancellationToken cancellationToken) => SendJsonMessageAsync(cancellationToken);
    }
}
