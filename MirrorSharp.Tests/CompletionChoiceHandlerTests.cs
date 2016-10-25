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

    public class CompletionChoiceHandlerTests {
        [Fact]
        public async Task ExecuteAsync_AppliesSelected_WhenIndexIsProvided() {
            var session = SessionFromTextWithCursor("class C { void M(object o) { o| } }");
            var completions = await TypeAndGetCompletionsAsync('.', session);
            var index = completions.Select((c, i) => new { c, i }).First(x => x.c.DisplayText.Contains("ToString")).i;

            await ExecuteHandlerAsync<CompletionChoiceHandler>(session, index);

            Assert.Equal(
                "class C { void M(object o) { o.ToString } }",
                session.SourceText.ToString()
            );
            Assert.Null(session.CurrentCompletionList);
        }

        [Fact]
        public async Task ExecuteAsync_CancelsCompletion_WhenXIsProvidedInsteadOfIndex() {
            var session = SessionFromTextWithCursor("class C { void M(object o) { o| } }");
            await TypeAndGetCompletionsAsync('.', session);

            var result = await ExecuteHandlerAsync<CompletionChoiceHandler, ChangesResult>(session, 'X');

            Assert.Null(result);
            Assert.Null(session.CurrentCompletionList);
        }

        private static async Task<IList<CompletionsResult.ResultCompletion>> TypeAndGetCompletionsAsync(char @char, WorkSession session) {
            return (await ExecuteHandlerAsync<TypeCharHandler, CompletionsResult>(session, @char)).Completions;
        }
    }
}
