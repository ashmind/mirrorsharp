using System;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Text;
using MirrorSharp.Internal.Results;

namespace MirrorSharp.Internal.Handlers {
    public class CompletionChoiceHandler : ICommandHandler {
        public IImmutableList<char> CommandIds { get; } = ImmutableList.Create('S');

        public async Task ExecuteAsync(ArraySegment<byte> data, WorkSession session, ICommandResultSender sender, CancellationToken cancellationToken) {
            if (data.Array[data.Offset] == (byte)'X') {
                // completion cancelled/dismissed
                session.CurrentCompletionList = null;
                return;
            }

            var itemIndex = FastConvert.Utf8ByteArrayToInt32(data);
            // ReSharper disable once PossibleNullReferenceException
            var completion = session.CurrentCompletionList;
            // ReSharper disable once PossibleNullReferenceException
            var item = completion.Items[itemIndex];
            var change = await session.CompletionService.GetChangeAsync(session.Document, item, cancellationToken: cancellationToken).ConfigureAwait(false);
            session.CurrentCompletionList = null;

            var textChanges = change.TextChanges.Insert(
                0, new TextChange(TextSpan.FromBounds(completion.DefaultSpan.Start, session.CursorPosition), "")
            );
            session.SourceText = session.SourceText.WithChanges(textChanges);

            var writer = sender.StartJsonMessage("changes");
            writer.WritePropertyStartArray("changes");
            foreach (var textChange in textChanges) {
                writer.WriteChange(textChange);
            }
            writer.WriteEndArray();
            await sender.SendJsonMessageAsync(cancellationToken).ConfigureAwait(false);
        }

        public bool CanChangeSession => true;
    }
}
