using System.Linq;
using System.Threading.Tasks;
using MirrorSharp.Internal.Handlers;
using MirrorSharp.Tests.Internal;
using MirrorSharp.Tests.Internal.Results;
using Xunit;

namespace MirrorSharp.Tests {
    using static TestHelper;

    public class ReplaceTextHandlerTests {
        [Theory]
        [InlineData("abc", "0:2:0::x", "xc", 0)]
        [InlineData("abc", "0:0:0::x", "xabc", 0)]
        [InlineData("abc", "0:0:2::", "abc", 2)]
        [InlineData("abc", "3:0:0::x:y", "abcx:y", 0)]
        [InlineData("abc", "0:0:0:test:x", "xabc", 0)]
        public async void ExecuteAsync_AddsSpecifiedCharacter(string initialText, string dataString, string expectedText, int expectedCursorPosition) {
            var session = SessionFromText(initialText);
            await ExecuteHandlerAsync<ReplaceTextHandler>(session, dataString);

            Assert.Equal(expectedText, session.SourceText.ToString());
            Assert.Equal(expectedCursorPosition, session.CursorPosition);
        }

        [Fact]
        public async Task ExecuteAsync_ProducesEmptySignatureHelp_IfCursorIsMovedOutsideOfSignatureSpan() {
            var session = SessionFromTextWithCursor(@"
                class C {
                    void M() {}
                    void T() { M| }
                }
            ");
            await ExecuteHandlerAsync<TypeCharHandler, SignaturesResult>(session, '(');
            var newPosition = session.CursorPosition - "T() { M(".Length;
            var result = await ExecuteHandlerAsync<ReplaceTextHandler, SignaturesResult>(
                session, Argument(newPosition, 0, "X", newPosition)
            );
            Assert.Equal(0, result.Signatures.Count);
        }

        [Fact]
        public async Task ExecuteAsync_ProducesSignatureHelpWithNewSelectedParameter_IfCursorIsMovedMovedBetweenParameters() {
            var session = SessionFromTextWithCursor(@"
                class C {
                    void M(int a, int b, int c) {}
                    void T() { M(1| }
                }
            ");
            await TypeCharsAsync(session, ",2,");

            var newPosition = session.CursorPosition - "2,".Length;
            var result = await ExecuteHandlerAsync<ReplaceTextHandler, SignaturesResult>(
                session, Argument(newPosition, "2,".Length, "", newPosition)
            );
            var signature = result.Signatures.Single();
            Assert.Equal("void C.M(int a, *int b*, int c)", signature.ToString());
        }

        [Fact]
        public async Task ExecuteAsync_ProducesCompletion_WhenCalledAfterCommitCharThatWouldHaveProducedIt() {
            var session = SessionFromTextWithCursor(@"class C { void M() { var x = | } }");

            await TypeCharsAsync(session, "in");
            await ExecuteHandlerAsync<CompletionStateHandler>(session, 1); // would complete "int" after echo
            await TypeCharsAsync(session, "."); // this was the commit char, happens *before* echo

            var newPosition = session.CursorPosition + ("int.".Length - "in.".Length);
            var result = await ExecuteHandlerAsync<ReplaceTextHandler, CompletionsResult>(
                session, Argument(session.CursorPosition - "in.".Length, "in".Length, "int", newPosition, trigger: "completion")
            );
            Assert.NotNull(result);
            Assert.Contains(nameof(int.Parse), result.Completions.Select(c => c.DisplayText));
        }

        private HandlerTestArgument Argument(int start, int length, string newText, int newCursorPosition, string trigger = "") {
            return $"{start}:{length}:{newCursorPosition}:{trigger}:{newText}";
        } 
    }
}
