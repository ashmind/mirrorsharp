using System;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Completion;
using Microsoft.CodeAnalysis.Text;
using MirrorSharp.Internal.Results;

namespace MirrorSharp.Internal.Handlers.Shared {
    public class CompletionSupport : ICompletionSupport {
        public Task ApplyTypedCharAsync(char @char, WorkSession session, ICommandResultSender sender, CancellationToken cancellationToken) {
            if (session.CurrentCompletionList != null && !session.CanRetriggerCompletion)
                return TaskEx.CompletedTask;

            var trigger = CompletionTrigger.CreateInsertionTrigger(@char);
            return CheckCompletionAsync(trigger, session, sender, cancellationToken);
        }

        public async Task ApplyCompletionSelectionAsync(int selectedIndex, WorkSession session, ICommandResultSender sender, CancellationToken cancellationToken) {
            // ReSharper disable once PossibleNullReferenceException
            var completion = session.CurrentCompletionList;
            // ReSharper disable once PossibleNullReferenceException
            var item = completion.Items[selectedIndex];
            var change = await session.CompletionService.GetChangeAsync(session.Document, item, cancellationToken: cancellationToken).ConfigureAwait(false);
            session.CurrentCompletionList = null;
            session.CanRetriggerCompletion = true;

            var textChanges = ReplaceIncompleteText(session, completion, change.TextChanges);

            session.SourceText = session.SourceText.WithChanges(textChanges);

            var writer = sender.StartJsonMessage("changes");
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
                session.SourceText = session.SourceText.WithChanges(new TextChange(new TextSpan(completion.DefaultSpan.Start, session.CursorPosition - completion.DefaultSpan.Start), ""));
            }
            return textChanges;
        }

        public Task ApplyCompletionStateChangeAsync(CompletionStateChange change, WorkSession session, ICommandResultSender sender, CancellationToken cancellationToken) {
            if (change == CompletionStateChange.Cancel) {
                // completion cancelled/dismissed
                session.CurrentCompletionList = null;
                session.CanRetriggerCompletion = false;
                return TaskEx.CompletedTask;
            }

            if (change == CompletionStateChange.Empty) {
                // completion is empty, can recomplete
                session.CanRetriggerCompletion = true;
                if (session.SourceText.Length == 0)
                    return TaskEx.CompletedTask;

                var trigger = CompletionTrigger.CreateInsertionTrigger(session.SourceText[session.CursorPosition - 1]);
                return CheckCompletionAsync(trigger, session, sender, cancellationToken);
            }

            if (change == CompletionStateChange.NonEmptyAfterEmpty) {
                // completion is non-empty again
                session.CanRetriggerCompletion = false;
                return TaskEx.CompletedTask;
            }

            throw new ArgumentOutOfRangeException($"Unsupported completion state change: {change}.");
        }
        
        private Task CheckCompletionAsync(CompletionTrigger trigger, WorkSession session, ICommandResultSender sender, CancellationToken cancellationToken) {
            if (!session.CompletionService.ShouldTriggerCompletion(session.SourceText, session.CursorPosition, trigger))
                return TaskEx.CompletedTask;

            return TriggerCompletionAsync(session, sender, cancellationToken, trigger);
        }

        private async Task TriggerCompletionAsync(WorkSession session, ICommandResultSender sender, CancellationToken cancellationToken, CompletionTrigger trigger) {
            var completionList = await session.CompletionService.GetCompletionsAsync(session.Document, session.CursorPosition, trigger, cancellationToken: cancellationToken).ConfigureAwait(false);
            if (completionList == null)
                return;

            session.CurrentCompletionList = completionList;
            await SendCompletionListAsync(completionList, sender, cancellationToken).ConfigureAwait(false);
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
