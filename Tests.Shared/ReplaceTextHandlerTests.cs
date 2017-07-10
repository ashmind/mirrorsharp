using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MirrorSharp.Internal;
using MirrorSharp.Testing;
using MirrorSharp.Testing.Internal;
using MirrorSharp.Testing.Internal.Results;
using Xunit;

// ReSharper disable HeapView.BoxingAllocation

namespace MirrorSharp.Tests {
    using static CommandIds;

    public class ReplaceTextHandlerTests {
        [Fact]
        public async void ExecuteAsync_AddsCompleteText_IfTextIsSplitIntoSeveralBuffers() {
            var driver = MirrorSharpTestDriver.New();
            await driver.SendAsync(ReplaceText, new[] { "0:0:0::x", "123456789", "123456789" });

            Assert.Equal("x123456789123456789", driver.Session.GetText());
        }

        [Fact]
        public async void ExecuteAsync_AddsCompleteText_IfTextIsSplitInTwoBuffersInTheMiddleOfUtf8Char() {
            var driver = MirrorSharpTestDriver.New();
            var bytes = Encoding.UTF8.GetBytes("0:0:0::☀");
            await driver.SendAsync(ReplaceText, new[] {
                bytes.Take(bytes.Length - 2).ToArray(),
                new[] { bytes[bytes.Length - 2], bytes[bytes.Length - 1] }
            });

            Assert.Equal("☀", driver.Session.GetText());
        }

        [Theory]
        [InlineData("abc", "0:2:0::x", "xc", 0)]
        [InlineData("abc", "0:0:0::x", "xabc", 0)]
        [InlineData("abc", "0:0:2::", "abc", 2)]
        [InlineData("abc", "3:0:0::x:y", "abcx:y", 0)]
        [InlineData("abc", "0:0:0:test:x", "xabc", 0)]
        public async void ExecuteAsync_AddsSpecifiedCharacter(string initialText, string dataString, string expectedText, int expectedCursorPosition) {
            var driver = MirrorSharpTestDriver.New().SetText(initialText);
            await driver.SendAsync(ReplaceText, dataString);

            Assert.Equal(expectedText, driver.Session.GetText());
            Assert.Equal(expectedCursorPosition, driver.Session.CursorPosition);
        }

        [Fact]
        public async Task ExecuteAsync_ProducesEmptySignatureHelp_IfCursorIsMovedOutsideOfSignatureSpan() {
            var driver = MirrorSharpTestDriver.New().SetTextWithCursor(@"
                class C {
                    void M() {}
                    void T() { M| }
                }
            ");
            await driver.SendAsync<SignaturesResult>(TypeChar, '(');
            var newPosition = driver.Session.CursorPosition - "T() { M(".Length;
            var result = await driver.SendAsync<SignaturesResult>(ReplaceText, Argument(newPosition, 0, "X", newPosition));
            Assert.Equal(0, result.Signatures.Count);
        }

        [Fact]
        public async Task ExecuteAsync_ProducesSignatureHelpWithNewSelectedParameter_IfCursorIsMovedMovedBetweenParameters() {
            var driver = MirrorSharpTestDriver.New().SetTextWithCursor(@"
                class C {
                    void M(int a, int b, int c) {}
                    void T() { M(1| }
                }
            ");
            await driver.SendTypeCharsAsync(",2,");

            var newPosition = driver.Session.CursorPosition - "2,".Length;
            var result = await driver.SendAsync<SignaturesResult>(ReplaceText, Argument(newPosition, "2,".Length, "", newPosition));
            var signature = result.Signatures.Single();
            Assert.Equal("void C.M(int a, *int b*, int c)", signature.ToString());
        }

        [Fact]
        public async Task ExecuteAsync_ProducesCompletion_WhenCalledAfterCommitCharThatWouldHaveProducedIt() {
            var driver = MirrorSharpTestDriver.New().SetTextWithCursor(@"class C { void M() { var x = | } }");

            await driver.SendTypeCharsAsync("in");
            await driver.SendAsync(CompletionState, 1); // would complete "int" after echo
            await driver.SendTypeCharsAsync("."); // this was the commit char, happens *before* echo

            var newPosition = driver.Session.CursorPosition + ("int.".Length - "in.".Length);
            var result = await driver.SendAsync<CompletionsResult>(
                ReplaceText, Argument(driver.Session.CursorPosition - "in.".Length, "in".Length, "int", newPosition, reason: "completion")
            );
            Assert.NotNull(result);
            Assert.Contains(nameof(int.Parse), result.Completions.Select(c => c.DisplayText));
        }

        [Fact]

        public async Task ExecuteAsync_ProducesSignatureHelp_WhenCalledAfterCommitCharThatWouldHaveProducedIt() {
            var driver = MirrorSharpTestDriver.New().SetTextWithCursor(@"class C { void M() { int.| } }");

            await driver.SendTypeCharsAsync("Pars");
            await driver.SendAsync(CompletionState, 1); // would complete "Parse" after echo
            await driver.SendTypeCharsAsync("("); // this was the commit char, happens *before* echo

            var newPosition = driver.Session.CursorPosition + ("Parse(".Length - "Pars(".Length);
            var result = await driver.SendAsync<SignaturesResult>(
                ReplaceText, Argument(driver.Session.CursorPosition - "Pars(".Length, "Pars".Length, "Parse", newPosition, reason: "completion")
            );
            var signature = result.Signatures.First(s => s.Selected);
            Assert.Equal("int int.Parse(*string s*)", signature.ToString());
        }

        private HandlerTestArgument Argument(int start, int length, string newText, int newCursorPosition, string reason = "") {
            return $"{start}:{length}:{newCursorPosition}:{reason}:{newText}";
        } 
    }
}