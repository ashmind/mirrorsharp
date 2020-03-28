using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using MirrorSharp.Internal;
using MirrorSharp.Testing;
using MirrorSharp.Testing.Internal.Results;
using MirrorSharp.Tests.Internal;
using Xunit;

// ReSharper disable HeapView.ClosureAllocation

namespace MirrorSharp.Tests {
    using static CommandIds;

    public class CompletionStateHandlerTests {
        [Fact]
        public async Task ExecuteAsync_ProducesChangeForSelectedCompletion() {
            var driver = MirrorSharpTestDriver.New().SetTextWithCursor("class C { void M(object o) { o| } }");
            var completions = await TypeAndGetCompletionsAsync('.', driver);

            var changes = await driver.SendWithRequiredResultAsync<ChangesResult>(CompletionState, IndexOf(completions, "ToString"));

            Assert.Equal("completion", changes.Reason);
            Assert.Equal(
                new[] { new { Start = 31, Length = 0, Text = "ToString" } },
                changes.Changes.Select(c => new { c.Start, c.Length, c.Text })
            );
            Assert.Null(driver.Session.CurrentCompletion.List);
        }

        [Fact]
        public async Task ExecuteAsync_ReplacesInterimTypedText() {
            var driver = MirrorSharpTestDriver.New().SetTextWithCursor("class C { void M(object o) { o| } }");
            var completions = await TypeAndGetCompletionsAsync('.', driver);
            await driver.SendTypeCharsAsync("To");

            var changes = await driver.SendWithRequiredResultAsync<ChangesResult>(CompletionState, IndexOf(completions, "ToString"));

            Assert.Equal(
                new[] { new { Start = 31, Length = 2, Text = "ToString" } },
                changes.Changes.Select(c => new { c.Start, c.Length, c.Text })
            );
            Assert.Null(driver.Session.CurrentCompletion.List);
        }

        [Fact]
        public async Task ExecuteAsync_CancelsCompletion_WhenXIsProvidedInsteadOfIndex() {
            var driver = MirrorSharpTestDriver.New().SetTextWithCursor("class C { void M(object o) { o| } }");
            await TypeAndGetCompletionsAsync('.', driver);

            var result = await driver.SendWithOptionalResultAsync<ChangesResult>(CompletionState, 'X');

            Assert.Null(result);
            Assert.Null(driver.Session.CurrentCompletion.List);
        }

        [Fact]
        public async Task ExecuteAsync_ForcesCompletion_WhenFIsProvidedInsteadOfIndex() {
            var driver = MirrorSharpTestDriver.New().SetTextWithCursor("class C { void M(object o) { o.| } }");

            var result = await driver.SendWithRequiredResultAsync<CompletionsResult>(CompletionState, 'F');

            Assert.NotNull(result);
            Assert.Equal(
                ObjectMembers.AllNames.OrderBy(n => n),
                result.Completions.Select(i => i.DisplayText).OrderBy(n => n)
            );
        }

        [Fact]
        public async Task ExecuteAsync_SendsItemInfo_WhenCompletionIsActiveAndIIsProvided() {
            var driver = MirrorSharpTestDriver.New(MirrorSharpOptionsWithXmlDocumentation.Instance)
                .SetTextWithCursor("class C { void M(object o) { o.| } }");

            var completions = await driver.SendWithRequiredResultAsync<CompletionsResult>(CompletionState, 'F');
            var getHashCodeIndex = completions.Completions
                .Select((c, index) => (c.DisplayText, index))
                .First(x => x.DisplayText == nameof(GetHashCode))
                .index;
            var result = await driver.SendWithRequiredResultAsync<CompletionsItemInfoResult>(CompletionState, "I" + getHashCodeIndex);

            Assert.NotNull(result);
            Assert.Equal(getHashCodeIndex, result.Index);
            Assert.Equal(
                "int object.GetHashCode()\r\nServes as the default hash function.",
                string.Join("", result.Parts)
            );
        }

        private static async Task<IList<CompletionsItem>> TypeAndGetCompletionsAsync(char @char, MirrorSharpTestDriver driver) {
            return (await driver.SendWithRequiredResultAsync<CompletionsResult>(TypeChar, @char)).Completions;
        }

        private static int IndexOf(IEnumerable<CompletionsItem> completions, string displayText) {
            return completions.Select((c, i) => new { c, i }).First(x => x.c.DisplayText?.Contains(displayText) ?? false).i;
        }
    }
}
