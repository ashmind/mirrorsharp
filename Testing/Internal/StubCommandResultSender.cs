using System.Buffers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MirrorSharp.Advanced;
using MirrorSharp.Advanced.EarlyAccess;
using MirrorSharp.Internal;
using MirrorSharp.Internal.Results;

namespace MirrorSharp.Testing.Internal {
    internal class StubCommandResultSender : ICommandResultSender {
        private readonly FastUtf8JsonWriter _writer = new FastUtf8JsonWriter(ArrayPool<byte>.Shared);
        private readonly IConnectionSendViewer? _sendViewer;
        private readonly WorkSession _session;

        public string? LastMessageTypeName { get; private set; }
        public string? LastMessageJson { get; private set; }

        public StubCommandResultSender(WorkSession session, IConnectionSendViewer? sendViewer) {
            _session = session;
            _sendViewer = sendViewer;
        }

        public IFastJsonWriter StartJsonMessage(string messageTypeName) {
            LastMessageTypeName = messageTypeName;
            _writer.Reset();
            _writer.WriteStartObject();
            return _writer;
        }

        public async Task SendJsonMessageAsync(CancellationToken cancellationToken) {
            _writer.WriteEndObject();
            if (_sendViewer != null)
                await _sendViewer.ViewDuringSendAsync(LastMessageTypeName!, _writer.WrittenSegment, _session, cancellationToken);
            LastMessageJson = Encoding.UTF8.GetString(_writer.WrittenSegment.Array, _writer.WrittenSegment.Offset, _writer.WrittenSegment.Count);
        }
    }
}
