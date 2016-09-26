using System;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;

namespace MirrorSharp.Internal.Commands {
    public class CommitCompletionHandler : ICommandHandler {
        public IImmutableList<char> CommandIds { get; } = ImmutableList.Create('S');

        public async Task ExecuteAsync(ArraySegment<byte> data, WorkSession session, ICommandResultSender sender, CancellationToken cancellationToken) {
            var itemIndex = FastConvert.Utf8ByteArrayToInt32(data);
            // ReSharper disable once PossibleNullReferenceException
            var item = session.CurrentCompletionList.Items[itemIndex];
            var change = await session.CompletionService.GetChangeAsync(session.Document, item, cancellationToken: cancellationToken).ConfigureAwait(false);

            var writer = sender.StartJsonMessage("changes");
            writer.WritePropertyStartArray("changes");
            foreach (var textChange in change.TextChanges) {
                writer.WriteStartObject();
                writer.WriteProperty("start", textChange.Span.Start);
                writer.WriteProperty("length", textChange.Span.Length);
                writer.WriteProperty("text", textChange.NewText);
                writer.WriteEndObject();
            }
            writer.WriteEndArray();
            await sender.SendJsonMessageAsync(cancellationToken).ConfigureAwait(false);
        }

        public bool CanChangeSession => false;
    }
}
