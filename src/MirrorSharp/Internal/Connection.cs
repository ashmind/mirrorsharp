using System;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MirrorSharp.Internal.Results;
using Newtonsoft.Json;

namespace MirrorSharp.Internal {
    public class Connection : IAsyncDisposable {
        private static class Commands {
            public const byte MoveCursor = (byte)'C';
            public const byte Replace = (byte)'R';
            public const byte TypeChar = (byte)'T';
            public const byte CommitCompletion = (byte)'S';
        }

        private readonly WebSocket _socket;
        private readonly IWorkSession _session;
        private readonly byte[] _inputByteBuffer = new byte[2048];
        private readonly byte[] _outputByteBuffer = new byte[4*1024];
        private readonly char[] _charBuffer = new char[2048];

        private readonly MemoryStream _jsonOutputStream;
        private readonly JsonWriter _jsonWriter;


        public Connection(WebSocket socket, IWorkSession session) {
            _socket = socket;
            _session = session;
            _jsonOutputStream = new MemoryStream(_outputByteBuffer);
            _jsonWriter = new JsonTextWriter(new StreamWriter(_jsonOutputStream));
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
                case Commands.CommitCompletion: return ProcessCommitCompletionAsync(Shift(data));
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

            await SendTypeCharResultAsync(result).ConfigureAwait(false);
        }

        private Task SendTypeCharResultAsync(TypeCharResult result) {
            var completions = result.Completions;

            var writer = StartJson();
            writer.WriteStartObject();
            writer.WriteProperty("type", "completions");
            writer.WriteStartObjectProperty("completions");
            writer.WritePropertyName("span");
            // ReSharper disable once PossibleNullReferenceException
            writer.WriteSpan(completions.DefaultSpan);
            writer.WriteStartArrayProperty("list");
            foreach (var item in completions.Items) {
                writer.WriteStartObject();
                writer.WriteProperty("displayText", item.DisplayText);
                writer.WriteStartArrayProperty("tags");
                foreach (var tag in item.Tags) {
                    writer.WriteValue(tag);
                }
                writer.WriteEndArray();
                if (item.Span != completions.DefaultSpan) {
                    writer.WritePropertyName("span");
                    writer.WriteSpan(item.Span);
                }
                writer.WriteEndObject();
            }
            writer.WriteEndArray();
            writer.WriteEndObject();
            writer.WriteEndObject();
            return SendJsonAsync();
        }

        private async Task ProcessCommitCompletionAsync(ArraySegment<byte> data) {
            var itemIndex = FastConvert.Utf8ByteArrayToInt32(data);
            var result = await _session.CommitCompletionAsync(itemIndex);
            await SendCommitCompletionResultAsync(result).ConfigureAwait(false);
        }

        private Task SendCommitCompletionResultAsync(CommitCompletionResult result) {
            var writer = StartJson();
            writer.WriteStartObject();
            writer.WriteProperty("type", "reset");
            writer.WriteProperty("text", result.NewText);
            writer.WriteProperty("cursor", result.NewCursorPosition);
            writer.WriteEndObject();
            return SendJsonAsync();
        }

        private JsonWriter StartJson() {
            _jsonOutputStream.Seek(0, SeekOrigin.Begin);
            return _jsonWriter;
        }

        private Task SendJsonAsync() {
            _jsonWriter.Flush();
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
