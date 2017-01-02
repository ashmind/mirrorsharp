using System;
using System.Linq;
using System.Threading.Tasks;
using MirrorSharp.Testing;
using MirrorSharp.Testing.Internal;
using MirrorSharp.Tests.Internal;
using MirrorSharp.Tests.Internal.Results;
using Xunit;

namespace MirrorSharp.Tests {
    using static CommandIds;

    public class TypeCharHandlerTests {
        [Theory]
        [InlineData('\u0216')]
        [InlineData('月')]
        [InlineData('❀')]
        public async Task ExecuteAsync_HandlesUnicodeChar(char @char) {
            var test = MirrorSharpTest.StartNew();
            await test.SendAsync(TypeChar, @char);
            Assert.Equal(@char.ToString(), test.Session.SourceText.ToString());
        }

        [Fact]
        public async Task ExecuteAsync_InsertsSingleChar() {
            var test = MirrorSharpTest.StartNew().SetTextWithCursor("class A| {}");
            await test.SendAsync(TypeChar, '1');

            Assert.Equal("class A1 {}", test.Session.SourceText.ToString());
        }

        [Fact]
        public async Task ExecuteAsync_MovesCursorBySingleChar() {
            var test = MirrorSharpTest.StartNew().SetTextWithCursor("class A| {}");
            var cursorPosition = test.Session.CursorPosition;
            await test.SendAsync(TypeChar, '1');

            Assert.Equal(cursorPosition + 1, test.Session.CursorPosition);
        }

        [Fact]
        public async Task ExecuteAsync_ProducesExpectedCompletion() {
            var test = MirrorSharpTest.StartNew().SetTextWithCursor(@"
                class A { public int x; }
                class B { void M(A a) { a| } }
            ");
            var result = await test.SendAsync<CompletionsResult>(TypeChar, '.');

            Assert.Equal(
                new[] { "x" }.Concat(ObjectMembers.AllNames).OrderBy(n => n),
                result.Completions.Select(i => i.DisplayText).OrderBy(n => n)
            );
        }

        [Fact]
        public async Task ExecuteAsync_ProducesExpectedCompletionWithSuggestionItem_InLambdaContext() {
            var test = MirrorSharpTest.StartNew().SetTextWithCursor(@"class C { void M() { System.Action a = | } }");
            var result = await test.SendAsync<CompletionsResult>(TypeChar, 's');

            Assert.Equal("<lambda expression>", result.Suggestion?.DisplayText);
        }

        [Fact]
        public async Task ExecuteAsync_ProducesExpectedCompletionWithMatchPriority_InEnumContext() {
            var test = MirrorSharpTest.StartNew().SetTextWithCursor(@"
                using System;
                class C { void M() { new DateTime().DayOfWeek =| } }
            ");
            var result = await test.SendAsync<CompletionsResult>(TypeChar, ' ');
            var dayOfWeek = result.Completions.FirstOrDefault(c => c.DisplayText == nameof(DayOfWeek));
            var maxPriority = result.Completions.Select(c => c.Priority ?? 0).Max();

            Assert.NotNull(dayOfWeek?.Priority);
            Assert.NotEqual(0, dayOfWeek.Priority);
            Assert.Equal(maxPriority, dayOfWeek.Priority);
        }

        [Theory]
        [InlineData("void M(int a) {}", new[] { "void C.M(int a)" })]
        [InlineData("void M(int a, string b) {}", new[] { "void C.M(int a, string b)" })]
        [InlineData("void M(int a) {} void M(string b) {}", new[] { "void C.M(int a)", "void C.M(string b)" })]
        public async Task ExecuteAsync_ProducesExpectedSignatureHelp(string methods, string[] expected) {
            var test = MirrorSharpTest.StartNew().SetTextWithCursor(@"
                class C {
                    " + methods + @"
                    void T() { M| }
                }
            ");
            var result = await test.SendAsync<SignaturesResult>(TypeChar, '(');
            Assert.Equal(expected, result.Signatures.Select(s => s.ToString(markSelected: false)));
        }

        [Theory]
        [InlineData("void M(int a, int b, int c) {}", "void C.M(int a, *int b*, int c)")]
        public async Task ExecuteAsync_ProducesSignatureHelpWithSelectedParameter(string methods, string expected) {
            var test = MirrorSharpTest.StartNew().SetTextWithCursor(@"
                class C {
                    " + methods + @"
                    void T() { M(1| }
                }
            ");
            var result = await test.SendAsync<SignaturesResult>(TypeChar, ',');
            var signature = result.Signatures.Single();
            Assert.Equal(expected, signature.ToString());
        }

        [Theory]
        [InlineData("void M(int a) {} void M(int a, int b) {}", "void C.M(int a, int b)")]
        public async Task ExecuteAsync_ProducesSignatureHelpWithSelectedSignature(string methods, string expectedSelected) {
            var test = MirrorSharpTest.StartNew().SetTextWithCursor(@"
                class C {
                    " + methods + @"
                    void T() { M(1| }
                }
            ");
            var result = await test.SendAsync<SignaturesResult>(TypeChar, ',');
            var selected = result.Signatures.Single(s => s.Selected);
            Assert.Equal(expectedSelected, string.Join("", selected.Parts.Select(p => p.Text)));
        }

        [Fact]
        public async Task ExecuteAsync_ProducesEmptySignatureHelp_OnClosingParenthesis() {
            var test = MirrorSharpTest.StartNew().SetTextWithCursor(@"
                class C {
                    void M() {}
                    void T() { M| }
                }
            ");
            await test.SendAsync(TypeChar, '(');
            var result = await test.SendAsync<SignaturesResult>(TypeChar, ')');
            Assert.Equal(0, result.Signatures.Count);
        }
    }
}
