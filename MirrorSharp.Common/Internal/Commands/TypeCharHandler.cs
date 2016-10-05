using System;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Completion;
using Microsoft.CodeAnalysis.Text;

namespace MirrorSharp.Internal.Commands {
    public class TypeCharHandler : ICommandHandler {
        public IImmutableList<char> CommandIds { get; } = ImmutableList.Create('C');

        public Task ExecuteAsync(ArraySegment<byte> data, WorkSession session, ICommandResultSender sender, CancellationToken cancellationToken) {
            var @char = FastConvert.Utf8ByteArrayToChar(data, session.Buffers.CharArray);
            session.SourceText = session.SourceText.WithChanges(
                new TextChange(new TextSpan(session.CursorPosition, 0), FastConvert.CharToString(@char))
            );
            session.CursorPosition += 1;
            if (session.CurrentCompletionList != null) {
                return TaskEx.CompletedTask;
            }

            var trigger = CompletionTrigger.CreateInsertionTrigger(@char);
            if (!session.CompletionService.ShouldTriggerCompletion(session.SourceText, session.CursorPosition, trigger))
                return TaskEx.CompletedTask;

            return TriggerCompletionAsync(session, sender, cancellationToken, trigger);
        }

        private async Task TriggerCompletionAsync(WorkSession session, ICommandResultSender sender, CancellationToken cancellationToken, CompletionTrigger trigger) {
            session.CurrentCompletionList = await session.CompletionService.GetCompletionsAsync(session.Document, session.CursorPosition, trigger, cancellationToken: cancellationToken).ConfigureAwait(false);
            if (session.CurrentCompletionList == null)
                return;

            await SendListAsync(session.CurrentCompletionList, sender, cancellationToken).ConfigureAwait(false);
        }

        private Task SendListAsync(CompletionList completionList, ICommandResultSender sender, CancellationToken cancellationToken) {
            var writer = sender.StartJsonMessage("completions");
            writer.WritePropertyStartObject("completions");
            writer.WritePropertyName("span");
            // ReSharper disable once PossibleNullReferenceException
            writer.WriteSpan(completionList.DefaultSpan);
            writer.WritePropertyStartArray("list");
            foreach (var item in completionList.Items) {
                writer.WriteStartObject();
                writer.WriteProperty("filterText", item.FilterText);
                writer.WriteProperty("displayText", item.DisplayText);
                writer.WritePropertyStartArray("tags");
                foreach (var tag in item.Tags) {
                    writer.WriteValue(tag.ToLowerInvariant());
                }
                writer.WriteEndArray();
                if (item.Span != completionList.DefaultSpan) {
                    writer.WritePropertyName("span");
                    writer.WriteSpan(item.Span);
                }
                writer.WriteEndObject();
            }
            writer.WriteEndArray();
            writer.WriteEndObject();
            return sender.SendJsonMessageAsync(cancellationToken);
        }

        public bool CanChangeSession => true;
    }
}
