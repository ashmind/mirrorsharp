using System.Linq;
using System.Threading.Tasks;
using MirrorSharp.Internal;
using MirrorSharp.Internal.Handlers;
using MirrorSharp.Tests.Internal;
using MirrorSharp.Tests.Internal.Results;
using Xunit;

namespace MirrorSharp.Tests {
    using static TestHelper;

    public class MoveCursorHandlerTests {
        [Theory]
        [InlineData("1", 1)]
        [InlineData("79", 79)]
        [InlineData("1234567890", 1234567890)]
        public async void ExecuteAsync_UpdatesSessionCursorPosition(string dataString, int expectedPosition) {
            var session = Session();
            await ExecuteHandlerAsync<MoveCursorHandler>(session, dataString);
            Assert.Equal(expectedPosition, session.CursorPosition);
        }

        [Fact]
        public async Task ExecuteAsync_ProducesEmptySignatureHelp_IfCursorIsMovedOutsideOfSignatureSpan() {
            var session = SessionFromTextWithCursor(@"
                class C {
                    void M() {}
                    void T() { M| }
                }
            ");
            var signatures = await ExecuteHandlerAsync<TypeCharHandler, SignaturesResult>(session, '(');
            var result = await ExecuteHandlerAsync<MoveCursorHandler, SignaturesResult>(session, signatures.Span.Start - 1);
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

            var result = await ExecuteHandlerAsync<MoveCursorHandler, SignaturesResult>(session, session.CursorPosition - 1);
            var signature = result.Signatures.Single();
            Assert.Equal("void C.M(int a, *int b*, int c)", signature.ToString());
        }
    }
}
