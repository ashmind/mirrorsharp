using System;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Completion;
using MirrorSharp.Internal.Results;
using Newtonsoft.Json;

namespace MirrorSharp.Internal {
    public class Connection : IDisposable {
        private static readonly Task Done = Task.FromResult((object)null);

        private static class Commands {
            public const byte TypeChar = (byte)'C';
            public const byte MoveCursor = (byte)'M';
            public const byte ReplaceProgress = (byte)'P';
            public const byte ReplaceLastOrOnly = (byte)'R';
            public const byte CommitCompletion = (byte)'S';
            public const byte SlowUpdate = (byte)'U';
        }

        private readonly WebSocket _socket;
        private readonly IWorkSession _session;
        private readonly byte[] _inputByteBuffer = new byte[2048];
        private readonly byte[] _outputByteBuffer = new byte[4*1024];
        private readonly char[] _charBuffer = new char[2048];

        private readonly MemoryStream _jsonOutputStream;
        private readonly JsonWriter _jsonWriter;
        private readonly IConnectionOptions _options;

        public Connection(WebSocket socket, IWorkSession session, IConnectionOptions options = null) {
            _socket = socket;
            _session = session;
            _jsonOutputStream = new MemoryStream(_outputByteBuffer);
            _jsonWriter = new JsonTextWriter(new StreamWriter(_jsonOutputStream));
            _options = options ?? new MirrorSharpOptions();
        }

        public bool IsConnected => _socket.State == WebSocketState.Open;

        public async Task ReceiveAndProcessAsync(CancellationToken cancellationToken) {
            try {
                await ReceiveAndProcessInternalAsync(cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex) {
                try {
                    await SendErrorAsync(ex.Message, cancellationToken).ConfigureAwait(false);
                }
                catch (Exception sendException) {
                    throw new AggregateException(ex, sendException);
                }
                throw;
            }
        }

        private async Task ReceiveAndProcessInternalAsync(CancellationToken cancellationToken) {
            var received = await _socket.ReceiveAsync(new ArraySegment<byte>(_inputByteBuffer), cancellationToken).ConfigureAwait(false);
            if (received.MessageType == WebSocketMessageType.Binary)
                throw new FormatException("Expected text data (received binary).");

            if (received.MessageType == WebSocketMessageType.Close) {
                await _socket.CloseAsync(received.CloseStatus ?? WebSocketCloseStatus.Empty, received.CloseStatusDescription, cancellationToken).ConfigureAwait(false);
                return;
            }

            await ProcessMessageAsync(new ArraySegment<byte>(_inputByteBuffer, 0, received.Count), cancellationToken).ConfigureAwait(false);
            if (_options.SendDebugCompareMessages)
                await SendDebugCompareAsync(_inputByteBuffer[0], cancellationToken).ConfigureAwait(false);
        }

        private Task ProcessMessageAsync(ArraySegment<byte> data, CancellationToken cancellationToken) {
            var command = data.Array[data.Offset];
            switch (command) {
                case Commands.ReplaceProgress:
                case Commands.ReplaceLastOrOnly: {
                    ProcessReplace(Shift(data));
                    return Done;
                }
                case Commands.MoveCursor: {
                    ProcessMoveCursor(Shift(data));
                    return Done;
                }
                case Commands.TypeChar: return ProcessTypeCharAsync(Shift(data), cancellationToken);
                case Commands.CommitCompletion: return ProcessCommitCompletionAsync(Shift(data), cancellationToken);
                case Commands.SlowUpdate: return ProcessSlowUpdateAsync(cancellationToken);
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

        private async Task ProcessTypeCharAsync(ArraySegment<byte> data, CancellationToken cancellationToken) {
            var @char = FastConvert.Utf8ByteArrayToChar(data, _charBuffer);

            var result = await _session.TypeCharAsync(@char, cancellationToken).ConfigureAwait(false);
            if (result.Completions == null)
                return;

            await SendTypeCharResultAsync(result, cancellationToken).ConfigureAwait(false);
        }

        private Task SendTypeCharResultAsync(TypeCharResult result, CancellationToken cancellationToken) {
            var completions = result.Completions;

            var writer = StartJsonMessage("completions");
            writer.WritePropertyStartObject("completions");
            writer.WritePropertyName("span");
            // ReSharper disable once PossibleNullReferenceException
            writer.WriteSpan(completions.DefaultSpan);
            writer.WritePropertyStartArray("list");
            foreach (var item in completions.Items) {
                writer.WriteStartObject();
                writer.WriteProperty("displayText", item.DisplayText);
                writer.WritePropertyStartArray("tags");
                foreach (var tag in item.Tags) {
                    writer.WriteValue(tag.ToLowerInvariant());
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
            return SendJsonMessageAsync(cancellationToken);
        }

        private async Task ProcessCommitCompletionAsync(ArraySegment<byte> data, CancellationToken cancellationToken) {
            var itemIndex = FastConvert.Utf8ByteArrayToInt32(data);
            var change = await _session.GetCompletionChangeAsync(itemIndex, cancellationToken);
            await SendCompletionChangeAsync(change, cancellationToken).ConfigureAwait(false);
        }

        private Task SendCompletionChangeAsync(CompletionChange change, CancellationToken cancellationToken) {
            var writer = StartJsonMessage("changes");
            writer.WritePropertyStartArray("changes");
            foreach (var textChange in change.TextChanges) {
                writer.WriteStartObject();
                writer.WriteProperty("start", textChange.Span.Start);
                writer.WriteProperty("length", textChange.Span.Length);
                writer.WriteProperty("text", textChange.NewText);
                writer.WriteEndObject();
            }
            writer.WriteEndArray();
            return SendJsonMessageAsync(cancellationToken);
        }

        private async Task ProcessSlowUpdateAsync(CancellationToken cancellationToken) {
            var update = await _session.GetSlowUpdateAsync(cancellationToken).ConfigureAwait(false);
            await SendSlowUpdateAsync(update, cancellationToken).ConfigureAwait(false);
        }

        private Task SendSlowUpdateAsync(SlowUpdateResult update, CancellationToken cancellationToken) {
            var writer = StartJsonMessage("slowUpdate");
            writer.WritePropertyStartArray("diagnostics");
            foreach (var diagnostic in update.Diagnostics) {
                writer.WriteStartObject();
                writer.WriteProperty("message", diagnostic.GetMessage());
                writer.WriteProperty("severity", diagnostic.Severity.ToString("G").ToLowerInvariant());
                writer.WritePropertyStartArray("tags");
                foreach (var tag in diagnostic.Descriptor.CustomTags) {
                    if (tag != WellKnownDiagnosticTags.Unnecessary)
                        continue;
                    writer.WriteValue(tag.ToLowerInvariant());
                }
                writer.WriteEndArray();
                writer.WritePropertyName("span");
                writer.WriteSpan(diagnostic.Location.SourceSpan);
                writer.WriteEndObject();
            }
            writer.WriteEndArray();
            return SendJsonMessageAsync(cancellationToken);
        }

        private Task SendDebugCompareAsync(byte command, CancellationToken cancellationToken) {
            if (command == Commands.CommitCompletion || command == Commands.SlowUpdate) // these cannot cause state changes
                return Done;

            if (command == Commands.ReplaceProgress) // let's wait for last one
                return Done;

            var writer = StartJsonMessage("debug:compare");
            if (command != Commands.MoveCursor)
                writer.WriteProperty("text", _session.SourceText.ToString());
            writer.WriteProperty("cursor", _session.CursorPosition);
            return SendJsonMessageAsync(cancellationToken);
        }

        private Task SendErrorAsync(string message, CancellationToken cancellationToken) {
            var writer = StartJsonMessage("error");
            writer.WriteProperty("message", message);
            return SendJsonMessageAsync(cancellationToken);
        }

        private JsonWriter StartJsonMessage(string messageType) {
            _jsonOutputStream.Seek(0, SeekOrigin.Begin);
            _jsonWriter.WriteStartObject();
            _jsonWriter.WriteProperty("type", messageType);
            return _jsonWriter;
        }

        private Task SendJsonMessageAsync(CancellationToken cancellationToken) {
            _jsonWriter.WriteEndObject();
            _jsonWriter.Flush();
            return _socket.SendAsync(
                new ArraySegment<byte>(_outputByteBuffer, 0, (int)_jsonOutputStream.Position),
                WebSocketMessageType.Text, true, cancellationToken
            );
        }

        public void Dispose() => _session.Dispose();
    }
}
