using System.Linq;
using System.Threading.Tasks;
using AshMind.Extensions;
using MirrorSharp.Internal;
using MirrorSharp.Internal.Commands;
using MirrorSharp.Tests.Internal;
using MirrorSharp.Tests.Internal.Results;
using Xunit;

namespace MirrorSharp.Tests {
    using static TestHelper;

    public class TypeCharHandlerTests {
        private static readonly string[] ObjectMemberNames = {
            nameof(Equals),
            nameof(GetHashCode),
            nameof(GetType),
            nameof(ToString)
        };

        [Theory]
        [InlineData('\u0216')]
        [InlineData('月')]
        [InlineData('❀')]
        public async Task ExecuteAsync_HandlesUnicodeChar(char @char) {
            var session = new WorkSession();
            await ExecuteHandlerAsync<TypeCharHandler>(session, @char);
            Assert.Equal(@char.ToString(), session.SourceText.ToString());
        }

        [Fact]
        public async Task ExecuteAsync_InsertsSingleChar() {
            var session = SessionFromTextWithCursor("class A| {}");
            await ExecuteHandlerAsync<TypeCharHandler>(session, '1');

            Assert.Equal("class A1 {}", session.SourceText.ToString());
        }

        [Fact]
        public async Task ExecuteAsync_MovesCursorBySingleChar() {
            var session = SessionFromTextWithCursor("class A| {}");
            var cursorPosition = session.CursorPosition;
            await ExecuteHandlerAsync<TypeCharHandler>(session, '1');

            Assert.Equal(cursorPosition + 1, session.CursorPosition);
        }

        [Fact]
        public async Task ExecuteAsync_ProducesExpectedCompletion() {
            var session = SessionFromTextWithCursor(@"
                class A { public int x; }
                class B { void M(A a) { a| } }
            ");
            var result = await ExecuteHandlerAsync<TypeCharHandler, CompletionsResult>(session, '.');

            Assert.Equal(
                new[] { "x" }.Concat(ObjectMemberNames).OrderBy(n => n),
                result.Completions.Select(i => i.DisplayText).OrderBy(n => n)
            );
        }

        [Theory]
        [InlineData("void M(int a) {}", new[] { "void C.M(int a)" })]
        [InlineData("void M(int a, string b) {}", new[] { "void C.M(int a, string b)" })]
        [InlineData("void M(int a) {} void M(string b) {}", new[] { "void C.M(int a)", "void C.M(string b)" })]
        public async Task ExecuteAsync_ProducesExpectedSignatureHelp(string methods, string[] expected) {
            var session = SessionFromTextWithCursor(@"
                class C {
                    " + methods + @"
                    void T() { M| }
                }
            ");
            var result = await ExecuteHandlerAsync<TypeCharHandler, SignaturesResult>(session, '(');
            Assert.Equal(
                expected,
                result.Signatures.Select(s => string.Join("", s.Select(p => p.Text)))
            );
        }

        [Theory]
        [InlineData("void M(int a, int b, int c) {}", "void C.M(int a, *int b*, int c)")]
        public async Task ExecuteAsync_ProducesSignatureHelpWithExpectedSelectedParameter(string methods, string expected) {
            var session = SessionFromTextWithCursor(@"
                class C {
                    " + methods + @"
                    void T() { M(1| }
                }
            ");
            var result = await ExecuteHandlerAsync<TypeCharHandler, SignaturesResult>(session, ',');
            var signature = result.Signatures.Single();
            var signatureText = string.Join("", signature.GroupAdjacentBy(p => p.Selected ? "*" : "").Select(g => g.Key + string.Join("", g.Select(p => p.Text)) + g.Key));
            Assert.Equal(expected, signatureText);
        }
    }
}
