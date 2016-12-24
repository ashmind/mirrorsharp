using System;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Completion;
using Microsoft.CodeAnalysis.Text;
using MirrorSharp.Internal.Handlers.Shared;
using MirrorSharp.Internal.Results;

namespace MirrorSharp.Internal.Handlers {
    public class TypeCharHandler : ICommandHandler {
        public IImmutableList<char> CommandIds { get; } = ImmutableList.Create('C');
        private readonly ISignatureHelpSupport _signatureHelp;

        public TypeCharHandler(ISignatureHelpSupport signatureHelp) {
            _signatureHelp = signatureHelp;
        }

        public async Task ExecuteAsync(ArraySegment<byte> data, WorkSession session, ICommandResultSender sender, CancellationToken cancellationToken) {
            var @char = FastConvert.Utf8ByteArrayToChar(data);
            session.SourceText = session.SourceText.WithChanges(
                new TextChange(new TextSpan(session.CursorPosition, 0), FastConvert.CharToString(@char))
            );
            session.CursorPosition += 1;

            await CheckCompletionAsync(@char, session, sender, cancellationToken).ConfigureAwait(false);
            await _signatureHelp.ApplyTypedCharAsync(@char, session, sender, cancellationToken).ConfigureAwait(false);
        }

        private Task CheckCompletionAsync(char @char, WorkSession session, ICommandResultSender sender, CancellationToken cancellationToken) {
            if (session.CurrentCompletionList != null)
                return TaskEx.CompletedTask;

            var trigger = CompletionTrigger.CreateInsertionTrigger(@char);
            if (!session.CompletionService.ShouldTriggerCompletion(session.SourceText, session.CursorPosition, trigger))
                return TaskEx.CompletedTask;

            return TriggerCompletionAsync(session, sender, cancellationToken, trigger);
        }

        private async Task TriggerCompletionAsync(WorkSession session, ICommandResultSender sender, CancellationToken cancellationToken, CompletionTrigger trigger) {
            session.CurrentCompletionList = await session.CompletionService.GetCompletionsAsync(session.Document, session.CursorPosition, trigger, cancellationToken: cancellationToken).ConfigureAwait(false);
            if (session.CurrentCompletionList == null)
                return;

            await SendCompletionListAsync(session.CurrentCompletionList, sender, cancellationToken).ConfigureAwait(false);
        }

        private Task SendCompletionListAsync(CompletionList completionList, ICommandResultSender sender, CancellationToken cancellationToken) {
            var writer = sender.StartJsonMessage("completions");
            writer.WritePropertyName("span");
            // ReSharper disable once PossibleNullReferenceException
            writer.WriteSpan(completionList.DefaultSpan);
            writer.WritePropertyStartArray("completions");
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
            return sender.SendJsonMessageAsync(cancellationToken);
        }
    }
}