using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MirrorSharp.Internal;
using MirrorSharp.Internal.Commands;
using Newtonsoft.Json;

namespace MirrorSharp.Tests.Internal {
    public class StubCommandResultSender : ICommandResultSender {
        private readonly FastJsonWriter _writer = new FastJsonWriter(new byte[4096]);

        public string LastMessageTypeName { get; private set; }
        public string LastMessageJson { get; private set; }

        public FastJsonWriter StartJsonMessage(string messageTypeName) {
            LastMessageTypeName = messageTypeName;
            _writer.Reset();
            _writer.WriteStartObject();
            return _writer;
        }

        public Task SendJsonMessageAsync(CancellationToken cancellationToken) {
            _writer.WriteEndObject();
            LastMessageJson = Encoding.UTF8.GetString(_writer.WrittenSegment.Array, _writer.WrittenSegment.Offset, _writer.WrittenSegment.Count);
            return Task.CompletedTask;
        }
    }
}
