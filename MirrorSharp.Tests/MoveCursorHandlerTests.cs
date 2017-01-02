using System.Linq;
using System.Threading.Tasks;
using MirrorSharp.Testing;
using MirrorSharp.Testing.Internal;
using MirrorSharp.Tests.Internal.Results;
using Xunit;

namespace MirrorSharp.Tests {
    using static CommandIds;

    public class MoveCursorHandlerTests {
        [Theory]
        [InlineData("1", 1)]
        [InlineData("79", 79)]
        [InlineData("1234567890", 1234567890)]
        public async void ExecuteAsync_UpdatesSessionCursorPosition(string dataString, int expectedPosition) {
            var test = MirrorSharpTest.StartNew();
            await test.SendAsync(MoveCursor, dataString);
            Assert.Equal(expectedPosition, test.Session.CursorPosition);
        }

        [Fact]
        public async Task ExecuteAsync_ProducesEmptySignatureHelp_IfCursorIsMovedOutsideOfSignatureSpan() {
            var test = MirrorSharpTest.StartNew().SetTextWithCursor(@"
                class C {
                    void M() {}
                    void T() { M| }
                }
            ");
            var signatures = await test.SendAsync<SignaturesResult>(TypeChar, '(');
            var result = await test.SendAsync<SignaturesResult>(MoveCursor, signatures.Span.Start - 1);
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

            var result = await test.SendAsync<SignaturesResult>(MoveCursor, test.Session.CursorPosition - 1);
            var signature = result.Signatures.Single();
            Assert.Equal("void C.M(int a, *int b*, int c)", signature.ToString());
        }
    }
}
