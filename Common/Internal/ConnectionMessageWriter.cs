using System;
using MirrorSharp.Advanced;

namespace MirrorSharp.Internal {
    internal class ConnectionMessageWriter : IDisposable {
        private readonly FastUtf8JsonWriter _jsonWriter;
        private string? _currentMessageTypeName;

        public ConnectionMessageWriter(FastUtf8JsonWriter jsonWriter) {
            _jsonWriter = jsonWriter;
        }

        public void WriteMessageStart(string messageTypeName) {
            _jsonWriter.Reset();
            _jsonWriter.WriteStartObject();
            _jsonWriter.WriteProperty("type", messageTypeName);
            _currentMessageTypeName = messageTypeName;
        }

        public void WriteErrorStart(string message) {
            WriteMessageStart("error");
            _jsonWriter.WriteProperty("message", message);
        }

        public void WriteMessageEnd() {
            _jsonWriter.WriteEndObject();
        }

        public ArraySegment<byte> WrittenSegment => _jsonWriter.WrittenSegment;
        public FastUtf8JsonWriter JsonWriter => _jsonWriter;
        public string? CurrentMessageTypeName => _currentMessageTypeName;

        public void Dispose() {
            _jsonWriter.Dispose();
        }
    }
}
