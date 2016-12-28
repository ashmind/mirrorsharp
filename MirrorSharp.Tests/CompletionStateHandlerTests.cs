using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MirrorSharp.Internal;
using MirrorSharp.Internal.Handlers;
using MirrorSharp.Tests.Internal;
using MirrorSharp.Tests.Internal.Results;
using Xunit;

namespace MirrorSharp.Tests {
    using static TestHelper;

    public class CompletionStateHandlerTests {
        [Fact]
        public async Task ExecuteAsync_ProducesChangeForSelectedCompletion() {
            var session = SessionFromTextWithCursor("class C { void M(object o) { o| } }");
            var completions = await TypeAndGetCompletionsAsync('.', session);
            var changes = await ExecuteHandlerAsync<CompletionStateHandler, ChangesResult>(session, IndexOf(completions, "ToString"));

            Assert.Equal("completion", changes.Reason);
            Assert.Equal(
                new[] { new { Start = 31, Length = 0, Text = "ToString" } },
                changes.Changes.Select(c => new { c.Start, c.Length, c.Text })
            );
            Assert.Null(session.Completion.CurrentList);
        }

        [Fact]
        public async Task ExecuteAsync_ReplacesInterimTypedText() {
            var session = SessionFromTextWithCursor("class C { void M(object o) { o| } }");
            var completions = await TypeAndGetCompletionsAsync('.', session);
            await TypeCharsAsync(session, "To");

            var changes = await ExecuteHandlerAsync<CompletionStateHandler, ChangesResult>(session, IndexOf(completions, "ToString"));

            Assert.Equal(
                new[] { new { Start = 31, Length = 2, Text = "ToString" } },
                changes.Changes.Select(c => new { c.Start, c.Length, c.Text })
            );
            Assert.Null(session.Completion.CurrentList);
        }

        [Fact]
        public async Task ExecuteAsync_CancelsCompletion_WhenXIsProvidedInsteadOfIndex() {
            var session = SessionFromTextWithCursor("class C { void M(object o) { o| } }");
            await TypeAndGetCompletionsAsync('.', session);

            var result = await ExecuteHandlerAsync<CompletionStateHandler, ChangesResult>(session, 'X');

            Assert.Null(result);
            Assert.Null(session.Completion.CurrentList);
        }

        [Fact]
        public async Task ExecuteAsync_ForcesCompletion_WhenFIsProvidedInsteadOfIndex() {
            var session = SessionFromTextWithCursor("class C { void M(object o) { o.| } }");

            var result = await ExecuteHandlerAsync<CompletionStateHandler, CompletionsResult>(session, 'F');
            
            Assert.NotNull(result);
            Assert.Equal(
                ObjectMemberNames.OrderBy(n => n),
                result.Completions.Select(i => i.DisplayText).OrderBy(n => n)
            );
        }

        private static async Task<IList<CompletionsResult.ResultItem>> TypeAndGetCompletionsAsync(char @char, WorkSession session) {
            return (await ExecuteHandlerAsync<TypeCharHandler, CompletionsResult>(session, @char)).Completions;
        }

        private static int IndexOf(IEnumerable<CompletionsResult.ResultItem> completions, string displayText) {
            return completions.Select((c, i) => new { c, i }).First(x => x.c.DisplayText.Contains(displayText)).i;
        }
    }
}
