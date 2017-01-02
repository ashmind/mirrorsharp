using System.Linq;
using System.Threading.Tasks;
using MirrorSharp.Testing;
using MirrorSharp.Testing.Internal;
using MirrorSharp.Tests.Internal.Results;
using Xunit;

namespace MirrorSharp.Tests {
    using static CommandIds;

    public class ReplaceTextHandlerTests {
        [Theory]
        [InlineData("abc", "0:2:0::x", "xc", 0)]
        [InlineData("abc", "0:0:0::x", "xabc", 0)]
        [InlineData("abc", "0:0:2::", "abc", 2)]
        [InlineData("abc", "3:0:0::x:y", "abcx:y", 0)]
        [InlineData("abc", "0:0:0:test:x", "xabc", 0)]
        public async void ExecuteAsync_AddsSpecifiedCharacter(string initialText, string dataString, string expectedText, int expectedCursorPosition) {
            var test = MirrorSharpTest.StartNew().SetText(initialText);
            await test.SendAsync(ReplaceText, dataString);

            Assert.Equal(expectedText, test.Session.SourceText.ToString());
            Assert.Equal(expectedCursorPosition, test.Session.CursorPosition);
        }

        [Fact]
        public async Task ExecuteAsync_ProducesEmptySignatureHelp_IfCursorIsMovedOutsideOfSignatureSpan() {
            var test = MirrorSharpTest.StartNew().SetTextWithCursor(@"
                class C {
                    void M() {}
                    void T() { M| }
                }
            ");
            await test.SendAsync<SignaturesResult>(TypeChar, '(');
            var newPosition = test.Session.CursorPosition - "T() { M(".Length;
            var result = await test.SendAsync<SignaturesResult>(ReplaceText, Argument(newPosition, 0, "X", newPosition));
            Assert.Equal(0, result.Signatures.Count);
        }

        [Fact]
        public async Task ExecuteAsync_ProducesSignatureHelpWithNewSelectedParameter_IfCursorIsMovedMovedBetweenParameters() {
            var test = MirrorSharpTest.StartNew().SetTextWithCursor(@"
                class C {
                    void M(int a, int b, int c) {}
                    void T() { M(1| }
                }
            ");
            await test.TypeCharsAsync(",2,");

            var newPosition = test.Session.CursorPosition - "2,".Length;
            var result = await test.SendAsync<SignaturesResult>(ReplaceText, Argument(newPosition, "2,".Length, "", newPosition));
            var signature = result.Signatures.Single();
            Assert.Equal("void C.M(int a, *int b*, int c)", signature.ToString());
        }

        [Fact]
        public async Task ExecuteAsync_ProducesCompletion_WhenCalledAfterCommitCharThatWouldHaveProducedIt() {
            var test = MirrorSharpTest.StartNew().SetTextWithCursor(@"class C { void M() { var x = | } }");

            await test.TypeCharsAsync("in");
            await test.SendAsync(CompletionState, 1); // would complete "int" after echo
            await test.TypeCharsAsync("."); // this was the commit char, happens *before* echo

            var newPosition = test.Session.CursorPosition + ("int.".Length - "in.".Length);
            var result = await test.SendAsync<CompletionsResult>(
                ReplaceText, Argument(test.Session.CursorPosition - "in.".Length, "in".Length, "int", newPosition, reason: "completion")
            );
            Assert.NotNull(result);
            Assert.Contains(nameof(int.Parse), result.Completions.Select(c => c.DisplayText));
        }

        [Fact]

        public async Task ExecuteAsync_ProducesSignatureHelp_WhenCalledAfterCommitCharThatWouldHaveProducedIt() {
            var test = MirrorSharpTest.StartNew().SetTextWithCursor(@"class C { void M() { int.| } }");

            await test.TypeCharsAsync("Pars");
            await test.SendAsync(CompletionState, 1); // would complete "Parse" after echo
            await test.TypeCharsAsync("("); // this was the commit char, happens *before* echo

            var newPosition = test.Session.CursorPosition + ("Parse(".Length - "Pars(".Length);
            var result = await test.SendAsync<SignaturesResult>(
                ReplaceText, Argument(test.Session.CursorPosition - "Pars(".Length, "Pars".Length, "Parse", newPosition, reason: "completion")
            );
            var signature = result.Signatures.First(s => s.Selected);
            Assert.Equal("int int.Parse(*string s*)", signature.ToString());
        }

        private HandlerTestArgument Argument(int start, int length, string newText, int newCursorPosition, string reason = "") {
            return $"{start}:{length}:{newCursorPosition}:{reason}:{newText}";
        } 
    }
}