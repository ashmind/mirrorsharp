using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MirrorSharp.Internal;
using MirrorSharp.Internal.Commands;
using MirrorSharp.Tests.Internal;
using Xunit;

namespace MirrorSharp.Tests {
    using static TestHelper;

    public class TypeCharHandlerTests : HandlerTestsBase<TypeCharHandler> {
        private static readonly string[] ObjectMemberNames = {
            nameof(Equals),
            nameof(GetHashCode),
            nameof(GetType),
            nameof(ToString)
        };

        [Theory]
        [InlineData('\u0216')]
        [InlineData('月')]
        public async Task ExecuteAsync_HandlesUnicodeChar(char @char) {
            var session = new WorkSession();
            await ExecuteAsync(session, ToByteArraySegment(@char));
            Assert.Equal(@char.ToString(), session.SourceText.ToString());
        }

        [Fact]
        public async Task ExecuteAsync_InsertsSingleChar() {
            var session = SessionFromTextWithCursor("class A| {}");
            await ExecuteAsync(session, ToByteArraySegment('1'));

            Assert.Equal("class A1 {}", session.SourceText.ToString());
        }

        [Fact]
        public async Task ExecuteAsync_MovesCursorBySingleChar() {
            var session = SessionFromTextWithCursor("class A| {}");
            var cursorPosition = session.CursorPosition;
            await ExecuteAsync(session, ToByteArraySegment('1'));

            Assert.Equal(cursorPosition + 1, session.CursorPosition);
        }

        [Fact]
        public async Task ExecuteAsync_ProducesExpectedCompletion() {
            var session = SessionFromTextWithCursor(@"
                class A { public int x; }
                class B { void M(A a) { a| } }
            ");
            var result = await ExecuteAndCaptureResultAsync<TypeCharResult>(session, ToByteArraySegment('.'));

            Assert.Equal(
                new[] { "x" }.Concat(ObjectMemberNames).OrderBy(n => n),
                result.Completions.List.Select(i => i.DisplayText).OrderBy(n => n)
            );
        }

        // ReSharper disable ClassNeverInstantiated.Local
        // ReSharper disable CollectionNeverUpdated.Local
        // ReSharper disable UnusedAutoPropertyAccessor.Local
        private class TypeCharResult {
            public ResultCompletions Completions { get; set; }
        }
        private class ResultCompletions {
            public IList<ResultCompletion> List { get; } = new List<ResultCompletion>();
        }
        private class ResultCompletion {
            public string DisplayText { get; set; }
        }
        // ReSharper restore ClassNeverInstantiated.Local
        // ReSharper restore CollectionNeverUpdated.Local
        // ReSharper restore UnusedAutoPropertyAccessor.Local
    }
}
