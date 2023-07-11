using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Completion;
using Microsoft.CodeAnalysis.Text;
using MirrorSharp.Advanced;
using MirrorSharp.Internal.Results;

namespace MirrorSharp.Internal.Handlers.Shared {
    internal class CompletionSupport : ICompletionSupport {
        private const string ChangeReasonCompletion = "completion";

        public Task ApplyTypedCharAsync(char @char, WorkSession session, ICommandResultSender sender, CancellationToken cancellationToken) {
            var current = session.CurrentCompletion;
            if (current.List != null)
                return Task.CompletedTask;

            if (current.ChangeEchoPending) {
                current.PendingChar = @char;
                return Task.CompletedTask;
            }
            var trigger = CompletionTrigger.CreateInsertionTrigger(@char);
            return CheckCompletionAsync(trigger, session, sender, cancellationToken);
        }

        public Task ApplyReplacedTextAsync(string reason, ITypedCharEffects typedCharEffects, WorkSession session, ICommandResultSender sender, CancellationToken cancellationToken) {
            if (reason != ChangeReasonCompletion)
                return Task.CompletedTask;

            var pendingChar = session.CurrentCompletion.PendingChar;
            session.CurrentCompletion.ResetPending();
            if (pendingChar == null)
                return Task.CompletedTask;

            return typedCharEffects.ApplyTypedCharAsync(pendingChar.Value, session, sender, cancellationToken);
        }

        public async Task SelectCompletionAsync(int selectedIndex, WorkSession session, ICommandResultSender sender, CancellationToken cancellationToken) {
            var current = session.CurrentCompletion;
            var completionList = current.List;
            if (completionList == null)
                throw new InvalidOperationException("Cannot select completion when completion list is not active.");

            var item = completionList.Items[selectedIndex];
            var change = await session.LanguageSession.GetCompletionChangeAsync(completionList.Span, item, cancellationToken: cancellationToken).ConfigureAwait(false);
            current.List = null;

            var textChange = ReplaceIncompleteText(session, completionList, change.TextChange);
            current.ChangeEchoPending = true;

            var writer = sender.StartJsonMessage("changes");
            writer.WriteProperty("reason", ChangeReasonCompletion);
            writer.WritePropertyStartArray("changes");
            writer.WriteChange(textChange);
            writer.WriteEndArray();
            await sender.SendJsonMessageAsync(cancellationToken).ConfigureAwait(false);
        }

        private static TextChange ReplaceIncompleteText(WorkSession session, CompletionList completionList, TextChange textChange) {
            var completionSpan = completionList.Span;
            if (session.CursorPosition <= completionSpan.Start)
                return textChange;

            var span = textChange.Span;
            var newStart = Math.Min(span.Start, completionSpan.Start);
            var newLength = Math.Max(span.End, session.CursorPosition) - newStart;
            return new TextChange(new TextSpan(newStart, newLength), textChange.NewText);
        }

        public async Task SendItemInfoAsync(int selectedIndex, WorkSession session, ICommandResultSender sender, CancellationToken cancellationToken) {
            var list = session.CurrentCompletion.List;
            if (list == null)
                return;

            var item = list.Items[selectedIndex];
            var description = await session.LanguageSession.GetCompletionDescriptionAsync(item, cancellationToken).ConfigureAwait(false);
            if (description == null)
                return;

            var writer = sender.StartJsonMessage("completionInfo");
            writer.WriteProperty("index", selectedIndex);
            writer.WritePropertyStartArray("parts");
            writer.WriteTaggedTexts(description.TaggedParts);
            writer.WriteEndArray();
            await sender.SendJsonMessageAsync(cancellationToken).ConfigureAwait(false);
        }

        public Task ForceCompletionAsync(WorkSession session, ICommandResultSender sender, CancellationToken cancellationToken) {
            return TriggerCompletionAsync(session, sender, cancellationToken, CompletionTrigger.Invoke);
        }

        public Task CancelCompletionAsync(WorkSession session, ICommandResultSender sender, CancellationToken cancellationToken) {
            session.CurrentCompletion.ResetPending();
            session.CurrentCompletion.List = null;
            return Task.CompletedTask;
        }

        private Task CheckCompletionAsync(CompletionTrigger trigger, WorkSession session, ICommandResultSender sender, CancellationToken cancellationToken) {
            if (!session.LanguageSession.ShouldTriggerCompletion(session.CursorPosition, trigger))
                return Task.CompletedTask;

            return TriggerCompletionAsync(session, sender, cancellationToken, trigger);
        }

        private async Task TriggerCompletionAsync(WorkSession session, ICommandResultSender sender, CancellationToken cancellationToken, CompletionTrigger trigger) {
            var completionList = await session.LanguageSession.GetCompletionsAsync(session.CursorPosition, trigger, cancellationToken: cancellationToken).ConfigureAwait(false);
            if (completionList == null || completionList.Items.IsEmpty)
                return;

            session.CurrentCompletion.ResetPending();
            session.CurrentCompletion.List = completionList;
            await SendCompletionListAsync(completionList, sender, cancellationToken).ConfigureAwait(false);
        }

        private Task SendCompletionListAsync(CompletionList completionList, ICommandResultSender sender, CancellationToken cancellationToken) {
            var completionSpan = completionList.Span;
            var writer = sender.StartJsonMessage("completions");

            writer.WriteProperty("commitChars", completionList.Rules.DefaultCommitCharacters);
            writer.WriteSpanProperty("span", completionSpan);

            var suggestionItem = completionList.SuggestionModeItem;
            if (suggestionItem != null) {
                writer.WritePropertyStartObject("suggestion");
                writer.WriteProperty("displayText", suggestionItem.DisplayText);
                writer.WriteEndObject();
            }

            writer.WritePropertyStartArray("completions");
            foreach (var item in completionList.Items) {
                writer.WriteStartObject();
                writer.WriteProperty("filterText", item.FilterText);
                writer.WriteProperty("displayText", item.DisplayText);
                writer.WriteTagsProperty("kinds", item.Tags);
                if (item.Span != completionSpan)
                    writer.WriteSpanProperty("span", item.Span);
                if (item.Rules.MatchPriority > 0)
                    writer.WriteProperty("priority", item.Rules.MatchPriority);
                writer.WriteEndObject();
            }
            writer.WriteEndArray();

            return sender.SendJsonMessageAsync(cancellationToken);
        }
    }
}
