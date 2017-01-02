using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MirrorSharp.Testing;
using MirrorSharp.Testing.Internal;
using MirrorSharp.Tests.Internal;
using MirrorSharp.Tests.Internal.Results;
using Xunit;

namespace MirrorSharp.Tests {
    using static CommandIds;

    public class CompletionStateHandlerTests {
        [Fact]
        public async Task ExecuteAsync_ProducesChangeForSelectedCompletion() {
            var test = MirrorSharpTest.StartNew().SetTextWithCursor("class C { void M(object o) { o| } }");
            var completions = await TypeAndGetCompletionsAsync('.', test);
            var changes = await test.SendAsync<ChangesResult>(CompletionState, IndexOf(completions, "ToString"));

            Assert.Equal("completion", changes.Reason);
            Assert.Equal(
                new[] { new { Start = 31, Length = 0, Text = "ToString" } },
                changes.Changes.Select(c => new { c.Start, c.Length, c.Text })
            );
            Assert.Null(test.Session.Completion.CurrentList);
        }

        [Fact]
        public async Task ExecuteAsync_ReplacesInterimTypedText() {
            var test = MirrorSharpTest.StartNew().SetTextWithCursor("class C { void M(object o) { o| } }");
            var completions = await TypeAndGetCompletionsAsync('.', test);
            await test.TypeCharsAsync("To");

            var changes = await test.SendAsync<ChangesResult>(CompletionState, IndexOf(completions, "ToString"));

            Assert.Equal(
                new[] { new { Start = 31, Length = 2, Text = "ToString" } },
                changes.Changes.Select(c => new { c.Start, c.Length, c.Text })
            );
            Assert.Null(test.Session.Completion.CurrentList);
        }

        [Fact]
        public async Task ExecuteAsync_CancelsCompletion_WhenXIsProvidedInsteadOfIndex() {
            var test = MirrorSharpTest.StartNew().SetTextWithCursor("class C { void M(object o) { o| } }");
            await TypeAndGetCompletionsAsync('.', test);

            var result = await test.SendAsync<ChangesResult>(CompletionState, 'X');

            Assert.Null(result);
            Assert.Null(test.Session.Completion.CurrentList);
        }

        [Fact]
        public async Task ExecuteAsync_ForcesCompletion_WhenFIsProvidedInsteadOfIndex() {
            var test = MirrorSharpTest.StartNew().SetTextWithCursor("class C { void M(object o) { o.| } }");

            var result = await test.SendAsync<CompletionsResult>(CompletionState, 'F');

            Assert.NotNull(result);
            Assert.Equal(
                ObjectMembers.AllNames.OrderBy(n => n),
                result.Completions.Select(i => i.DisplayText).OrderBy(n => n)
            );
        }

        private static async Task<IList<CompletionsResult.ResultItem>> TypeAndGetCompletionsAsync(char @char, MirrorSharpTest test) {
            return (await test.SendAsync<CompletionsResult>(TypeChar, @char)).Completions;
        }

        private static int IndexOf(IEnumerable<CompletionsResult.ResultItem> completions, string displayText) {
            return completions.Select((c, i) => new { c, i }).First(x => x.c.DisplayText.Contains(displayText)).i;
        }
    }
}
