using System;
using System.Threading;
using System.Threading.Tasks;
using MirrorSharp.Internal.Results;

namespace MirrorSharp.Internal.Handlers {
    internal class RequestSelfDebugDataHandler : ICommandHandler {
        public char CommandId => 'Y';

        public Task ExecuteAsync(ArraySegment<byte> data, WorkSession session, ICommandResultSender sender, CancellationToken cancellationToken) {
            var writer = sender.StartJsonMessage("self:debug");
            writer.WritePropertyStartArray("log");
            // ReSharper disable once PossibleNullReferenceException
            foreach (var entry in session.SelfDebug.GetLogEntries()) {
                writer.WriteStartObject();
                writer.WriteProperty("time", entry.DateTime.ToString("yyyy-MM-ddTHH:mm:ss.fffK"));
                writer.WriteProperty("event", entry.EventType);
                writer.WriteProperty("message", entry.Message);
                writer.WriteProperty("cursor", entry.CursorPosition);
                writer.WriteProperty("text", entry.SourceText.ToString());
                writer.WriteEndObject();
            }
            writer.WriteEndArray();
            return sender.SendJsonMessageAsync(cancellationToken);
        }
    }
}