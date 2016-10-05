using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MirrorSharp.Internal.Commands;
using Newtonsoft.Json;

namespace MirrorSharp.Tests.Internal {
    public class StubCommandResultSender : ICommandResultSender {
        private readonly StringBuilder _currentMessageBuilder = new StringBuilder();

        public string LastMessageTypeName { get; private set; }
        public string LastMessageJson { get; private set; }

        public JsonWriter StartJsonMessage(string messageTypeName) {
            LastMessageTypeName = messageTypeName;
            _currentMessageBuilder.Clear();
            return new JsonTextWriter(new StringWriter(_currentMessageBuilder));
        }

        public Task SendJsonMessageAsync(CancellationToken cancellationToken) {
            LastMessageJson = "{ " + _currentMessageBuilder + " }";
            return Task.CompletedTask;
        }
    }
}
