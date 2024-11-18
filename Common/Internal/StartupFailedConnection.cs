
using System;
using System.Buffers;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace MirrorSharp.Internal;

internal class StartupFailedConnection : IConnection {
    public static int InputBufferSize => 4096;

    private readonly ArrayPool<byte> _bufferPool;
    private readonly WebSocket _socket;
    private readonly Exception _startupException;
    private readonly byte[] _inputBuffer;

    private readonly ConnectionMessageWriter _messageWriter;
    private readonly IConnectionOptions? _options;

    public StartupFailedConnection(
        WebSocket socket,
        Exception startupException,
        ArrayPool<byte> bufferPool,
        ConnectionMessageWriter messageWriter,
        IConnectionOptions? options
    ) {
        _socket = socket;
        _startupException = startupException;
        _messageWriter = messageWriter;
        _options = options;
        _bufferPool = bufferPool;
        _inputBuffer = bufferPool.Rent(InputBufferSize);
    }

    public bool IsConnected => _socket.State == WebSocketState.Open;

    public async Task ReceiveAndProcessAsync(CancellationToken cancellationToken) {
        var first = await _socket.ReceiveAsync(new ArraySegment<byte>(_inputBuffer), cancellationToken).ConfigureAwait(false);
        if (first.MessageType == WebSocketMessageType.Close) {
            await _socket.CloseAsync(first.CloseStatus ?? WebSocketCloseStatus.Empty, first.CloseStatusDescription, cancellationToken).ConfigureAwait(false);
            return;
        }

        if (!first.EndOfMessage)
            await ReceiveToEndAsync(cancellationToken).ConfigureAwait(false);

        var error = (_options?.IncludeExceptionDetails ?? false)
            ? _startupException.ToString()
            : "A server error has occurred during startup.";

        _messageWriter.WriteErrorStart(error);
        _messageWriter.WriteMessageEnd();
        await _socket.SendAsync(
            _messageWriter.WrittenSegment,
            WebSocketMessageType.Text, true, cancellationToken
        );
    }

    private async Task ReceiveToEndAsync(CancellationToken cancellationToken) {
        while (!(await _socket.ReceiveAsync(new ArraySegment<byte>(_inputBuffer), cancellationToken).ConfigureAwait(false)).EndOfMessage) {
        }
    }

    public void Dispose() {
        _bufferPool.Return(_inputBuffer);
        _messageWriter.Dispose();
    }
}