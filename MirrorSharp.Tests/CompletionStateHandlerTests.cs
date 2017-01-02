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
            var driver = MirrorSharpTestDriver.New().SetTextWithCursor("class C { void M(object o) { o| } }");
            var completions = await TypeAndGetCompletionsAsync('.', driver);
            var changes = await driver.SendAsync<ChangesResult>(CompletionState, IndexOf(completions, "ToString"));

            Assert.Equal("completion", changes.Reason);
            Assert.Equal(
                new[] { new { Start = 31, Length = 0, Text = "ToString" } },
                changes.Changes.Select(c => new { c.Start, c.Length, c.Text })
            );
            Assert.Null(driver.Session.Completion.CurrentList);
        }

        [Fact]
        public async Task ExecuteAsync_ReplacesInterimTypedText() {
            var driver = MirrorSharpTestDriver.New().SetTextWithCursor("class C { void M(object o) { o| } }");
            var completions = await TypeAndGetCompletionsAsync('.', driver);
            await driver.TypeCharsAsync("To");

            var changes = await driver.SendAsync<ChangesResult>(CompletionState, IndexOf(completions, "ToString"));

            Assert.Equal(
                new[] { new { Start = 31, Length = 2, Text = "ToString" } },
                changes.Changes.Select(c => new { c.Start, c.Length, c.Text })
            );
            Assert.Null(driver.Session.Completion.CurrentList);
        }

        [Fact]
        public async Task ExecuteAsync_CancelsCompletion_WhenXIsProvidedInsteadOfIndex() {
            var driver = MirrorSharpTestDriver.New().SetTextWithCursor("class C { void M(object o) { o| } }");
            await TypeAndGetCompletionsAsync('.', driver);

            var result = await driver.SendAsync<ChangesResult>(CompletionState, 'X');

            Assert.Null(result);
            Assert.Null(driver.Session.Completion.CurrentList);
        }

        [Fact]
        public async Task ExecuteAsync_ForcesCompletion_WhenFIsProvidedInsteadOfIndex() {
            var driver = MirrorSharpTestDriver.New().SetTextWithCursor("class C { void M(object o) { o.| } }");

            var result = await driver.SendAsync<CompletionsResult>(CompletionState, 'F');

            Assert.NotNull(result);
            Assert.Equal(
                ObjectMembers.AllNames.OrderBy(n => n),
                result.Completions.Select(i => i.DisplayText).OrderBy(n => n)
            );
        }

        private static async Task<IList<CompletionsResult.ResultItem>> TypeAndGetCompletionsAsync(char @char, MirrorSharpTestDriver driver) {
            return (await driver.SendAsync<CompletionsResult>(TypeChar, @char)).Completions;
        }

        private static int IndexOf(IEnumerable<CompletionsResult.ResultItem> completions, string displayText) {
            return completions.Select((c, i) => new { c, i }).First(x => x.c.DisplayText.Contains(displayText)).i;
        }
    }
}
