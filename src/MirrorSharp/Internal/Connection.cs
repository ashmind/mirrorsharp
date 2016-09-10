using System;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace MirrorSharp.Internal {
    public class Connection : IAsyncDisposable {
        private static class Commands {
            public const byte MoveCursor = (byte)'C';
            public const byte Replace = (byte)'R';
            public const byte TypeChar = (byte)'T';
        }

        private readonly WebSocket _socket;
        private readonly IWorkSession _session;
        private readonly byte[] _inputByteBuffer = new byte[2048];
        private readonly byte[] _outputByteBuffer = new byte[2048];
        private readonly char[] _charBuffer = new char[2048];

        private readonly JsonSerializer _jsonSerializer = new JsonSerializer();
        private readonly MemoryStream _jsonOutputStream;
        private readonly JsonWriter _jsonOutputWriter;

        public Connection(WebSocket socket, IWorkSession session) {
            _socket = socket;
            _session = session;
            _jsonOutputStream = new MemoryStream(_outputByteBuffer);
            _jsonOutputWriter = new JsonTextWriter(new StreamWriter(_jsonOutputStream));
        }

        public bool IsConnected => _socket.State == WebSocketState.Open;

        public async Task ReceiveAndProcessAsync() {
            try {
                await ReceiveAndProcessInternalAsync().ConfigureAwait(false);
            }
            catch (Exception ex) {
                try {
                    await SendTextAsync("RM-ERR-??: " + ex.Message).ConfigureAwait(false);
                }
                catch (Exception sendException) {
                    throw new AggregateException(ex, sendException);
                }
                throw;
            }
        }

        private async Task ReceiveAndProcessInternalAsync() {
            var received = await _socket.ReceiveAsync(new ArraySegment<byte>(_inputByteBuffer), CancellationToken.None).ConfigureAwait(false);
            if (received.MessageType == WebSocketMessageType.Binary)
                throw new FormatException("Expected text data (received binary).");

            if (received.MessageType == WebSocketMessageType.Close)
                return;

            await ProcessMessageAsync(new ArraySegment<byte>(_inputByteBuffer, 0, received.Count)).ConfigureAwait(false);
        }

        private Task ProcessMessageAsync(ArraySegment<byte> data) {
            var command = data.Array[data.Offset];
            switch (command) {
                case Commands.Replace: {
                    ProcessReplace(Shift(data));
                    return Task.CompletedTask;
                }
                case Commands.MoveCursor: {
                    ProcessMoveCursor(Shift(data));
                    return Task.CompletedTask;
                }
                case Commands.TypeChar: return ProcessTypeCharAsync(Shift(data));
                default: throw new FormatException($"Unknown command: '{(char)command}'.");
            }
        }

        private ArraySegment<byte> Shift(ArraySegment<byte> data) {
            return new ArraySegment<byte>(data.Array, data.Offset + 1, data.Count - 1);
        }

        private void ProcessReplace(ArraySegment<byte> data) {
            var endOffset = data.Offset + data.Count - 1;
            var partStart = data.Offset;
            int? start = null;
            int? length = null;
            int? cursorPosition = null;

            for (var i = data.Offset; i <= endOffset; i++) {
                if (data.Array[i] != (byte)':')
                    continue;

                var part = new ArraySegment<byte>(data.Array, partStart, i - partStart);
                if (start == null) {
                    start = FastConvert.Utf8ByteArrayToInt32(part);
                    partStart = i + 1;
                    continue;
                }

                if (length == null) {
                    length = FastConvert.Utf8ByteArrayToInt32(part);
                    partStart = i + 1;
                    continue;
                }

                cursorPosition = FastConvert.Utf8ByteArrayToInt32(part);
                partStart = i + 1;
                break;
            }
            if (start == null || length == null || cursorPosition == null)
                throw new Exception("Command 'R' must be in a format 'Rstart:length:cursor:text'.");

            var text = Encoding.UTF8.GetString(data.Array, partStart, endOffset - partStart + 1);
            _session.ReplaceText(start.Value, length.Value, text, cursorPosition.Value);
        }

        private void ProcessMoveCursor(ArraySegment<byte> data) {
            var cursorPosition = FastConvert.Utf8ByteArrayToInt32(data);
            _session.MoveCursor(cursorPosition);
        }

        private async Task ProcessTypeCharAsync(ArraySegment<byte> data) {
            var @char = FastConvert.Utf8ByteArrayToChar(data, _charBuffer);

            var result = await _session.TypeCharAsync(@char).ConfigureAwait(false);
            if (result.Completions == null)
                return;

            await SendJsonAsync(new {
                type = "completions",
                completions = result.Completions.Items.Select(i => $"[{string.Join(",", i.Tags)}] {i.DisplayText}")
            }).ConfigureAwait(false);
        }

        private Task SendJsonAsync(object value) {
            _jsonOutputStream.Seek(0, SeekOrigin.Begin);
            _jsonSerializer.Serialize(_jsonOutputWriter, value);
            _jsonOutputWriter.Flush();
            return SendOutputBufferAsync((int)_jsonOutputStream.Position);
        }

        private Task SendTextAsync(string text) {
            var byteCount = Encoding.UTF8.GetBytes(text, 0, text.Length, _outputByteBuffer, 0);
            return SendOutputBufferAsync(byteCount);
        }

        private Task SendOutputBufferAsync(int byteCount) {
            return _socket.SendAsync(new ArraySegment<byte>(_outputByteBuffer, 0, byteCount), WebSocketMessageType.Text, true, CancellationToken.None);
        }

        public Task DisposeAsync() => _session.DisposeAsync();
    }
}
