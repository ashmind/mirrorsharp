using System;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Completion;
using Microsoft.CodeAnalysis.Text;
using MirrorSharp.Internal.Results;

namespace MirrorSharp.Internal.Handlers.Shared {
    internal class CompletionSupport : ICompletionSupport {
        private const string ChangeReasonCompletion = "completion";

        public Task ApplyTypedCharAsync(char @char, WorkSession session, ICommandResultSender sender, CancellationToken cancellationToken) {
            if (session.Completion.CurrentList != null)
                return TaskEx.CompletedTask;

            var trigger = CompletionTrigger.CreateInsertionTrigger(@char);
            if (session.Completion.ChangeEchoPending) {
                session.Completion.PendingTrigger = trigger;
                return TaskEx.CompletedTask;
            }
            return CheckCompletionAsync(trigger, session, sender, cancellationToken);
        }

        public Task ApplyReplacedTextAsync(string reason, WorkSession session, ICommandResultSender sender, CancellationToken cancellationToken) {
            if (reason != ChangeReasonCompletion)
                return TaskEx.CompletedTask;

            session.Completion.ChangeEchoPending = false;
            var pendingTrigger = session.Completion.PendingTrigger;
            if (pendingTrigger == null)
                return TaskEx.CompletedTask;

            session.Completion.PendingTrigger = null;
            return CheckCompletionAsync(pendingTrigger.Value, session, sender, cancellationToken);
        }

        public async Task ApplyCompletionSelectionAsync(int selectedIndex, WorkSession session, ICommandResultSender sender, CancellationToken cancellationToken) {
            // ReSharper disable once PossibleNullReferenceException
            var completion = session.Completion.CurrentList;
            // ReSharper disable once PossibleNullReferenceException
            var item = completion.Items[selectedIndex];
            var change = await session.Completion.Service.GetChangeAsync(session.Document, item, cancellationToken: cancellationToken).ConfigureAwait(false);
            session.Completion.CurrentList = null;

            var textChanges = ReplaceIncompleteText(session, completion, change.TextChanges);
            session.Completion.ChangeEchoPending = true;

            var writer = sender.StartJsonMessage("changes");
            writer.WriteProperty("reason", ChangeReasonCompletion);
            writer.WritePropertyStartArray("changes");
            foreach (var textChange in textChanges) {
                writer.WriteChange(textChange);
            }
            writer.WriteEndArray();
            await sender.SendJsonMessageAsync(cancellationToken).ConfigureAwait(false);
        }

        private static ImmutableArray<TextChange> ReplaceIncompleteText(WorkSession session, CompletionList completion, ImmutableArray<TextChange> textChanges) {
            if (session.CursorPosition <= completion.DefaultSpan.Start)
                return textChanges;

            if (textChanges.Length == 1) {
                // optimization
                var span = textChanges[0].Span;
                var newStart = Math.Min(span.Start, completion.DefaultSpan.Start);
                var newLength = Math.Max(span.End, session.CursorPosition) - newStart;
                textChanges = ImmutableArray.Create(new TextChange(new TextSpan(newStart, newLength), textChanges[0].NewText));
            }
            else {
                textChanges = textChanges.Insert(0, new TextChange(new TextSpan(completion.DefaultSpan.Start, session.CursorPosition - completion.DefaultSpan.Start), ""));
            }
            return textChanges;
        }

        public Task ApplyCompletionCancellationAsync(WorkSession session, ICommandResultSender sender, CancellationToken cancellationToken) {
            session.Completion.CurrentList = null;
            session.Completion.ChangeEchoPending = false;
            session.Completion.PendingTrigger = null;
            return TaskEx.CompletedTask;
        }

        public Task ApplyCompletionForceAsync(WorkSession session, ICommandResultSender sender, CancellationToken cancellationToken) {
            return TriggerCompletionAsync(session, sender, cancellationToken, CompletionTrigger.Default);
        }

        private Task CheckCompletionAsync(CompletionTrigger trigger, WorkSession session, ICommandResultSender sender, CancellationToken cancellationToken) {
            if (!session.Completion.Service.ShouldTriggerCompletion(session.SourceText, session.CursorPosition, trigger))
                return TaskEx.CompletedTask;

            return TriggerCompletionAsync(session, sender, cancellationToken, trigger);
        }

        private async Task TriggerCompletionAsync(WorkSession session, ICommandResultSender sender, CancellationToken cancellationToken, CompletionTrigger trigger) {
            var completionList = await session.Completion.Service.GetCompletionsAsync(session.Document, session.CursorPosition, trigger, cancellationToken: cancellationToken).ConfigureAwait(false);
            if (completionList == null)
                return;

            session.Completion.CurrentList = completionList;
            session.Completion.ChangeEchoPending = false;
            session.Completion.PendingTrigger = null;
            await SendCompletionListAsync(completionList, sender, cancellationToken).ConfigureAwait(false);
        }

        private Task SendCompletionListAsync(CompletionList completionList, ICommandResultSender sender, CancellationToken cancellationToken) {
            var writer = sender.StartJsonMessage("completions");

            writer.WriteProperty("commitChars", new CharListString(completionList.Rules.DefaultCommitCharacters));
            writer.WritePropertyName("span");
            writer.WriteSpan(completionList.DefaultSpan);

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
                writer.WritePropertyStartArray("tags");
                foreach (var tag in item.Tags) {
                    writer.WriteValue(tag.ToLowerInvariant());
                }
                writer.WriteEndArray();
                if (item.Span != completionList.DefaultSpan) {
                    writer.WritePropertyName("span");
                    writer.WriteSpan(item.Span);
                }
                if (item.Rules.MatchPriority > 0)
                    writer.WriteProperty("priority", item.Rules.MatchPriority);
                writer.WriteEndObject();
            }
            writer.WriteEndArray();

            return sender.SendJsonMessageAsync(cancellationToken);
        }
    }
}
