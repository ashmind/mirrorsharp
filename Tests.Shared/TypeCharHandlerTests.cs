using System;
using System.Linq;
using System.Threading.Tasks;
using MirrorSharp.Internal;
using MirrorSharp.Testing;
using MirrorSharp.Testing.Internal.Results;
using MirrorSharp.Tests.Internal;
using Xunit;

namespace MirrorSharp.Tests {
    using static CommandIds;

    public class TypeCharHandlerTests {
        [Theory]
        [InlineData('\u0216')]
        [InlineData('月')]
        [InlineData('❀')]
        public async Task ExecuteAsync_HandlesUnicodeChar(char @char) {
            var driver = MirrorSharpTestDriver.New();
            await driver.SendAsync(TypeChar, @char);
            Assert.Equal(@char.ToString(), driver.Session.SourceText.ToString());
        }

        [Fact]
        public async Task ExecuteAsync_InsertsSingleChar() {
            var driver = MirrorSharpTestDriver.New().SetSourceTextWithCursor("class A| {}");
            await driver.SendAsync(TypeChar, '1');

            Assert.Equal("class A1 {}", driver.Session.SourceText.ToString());
        }

        [Fact]
        public async Task ExecuteAsync_MovesCursorBySingleChar() {
            var driver = MirrorSharpTestDriver.New().SetSourceTextWithCursor("class A| {}");
            var cursorPosition = driver.Session.CursorPosition;
            await driver.SendAsync(TypeChar, '1');

            Assert.Equal(cursorPosition + 1, driver.Session.CursorPosition);
        }

        [Fact]
        public async Task ExecuteAsync_ProducesExpectedCompletion() {
            var driver = MirrorSharpTestDriver.New().SetSourceTextWithCursor(@"
                class A { public int x; }
                class B { void M(A a) { a| } }
            ");
            var result = await driver.SendAsync<CompletionsResult>(TypeChar, '.');

            Assert.Equal(
                new[] { "x" }.Concat(ObjectMembers.AllNames).OrderBy(n => n),
                result.Completions.Select(i => i.DisplayText).OrderBy(n => n)
            );
        }

        [Fact]
        public async Task ExecuteAsync_ProducesExpectedCompletionWithSuggestionItem_InLambdaContext() {
            var driver = MirrorSharpTestDriver.New().SetSourceTextWithCursor(@"class C { void M() { System.Action a = | } }");
            var result = await driver.SendAsync<CompletionsResult>(TypeChar, 's');

            Assert.Equal("<lambda expression>", result.Suggestion?.DisplayText);
        }

        [Fact]
        public async Task ExecuteAsync_ProducesExpectedCompletionWithMatchPriority_InEnumContext() {
            var driver = MirrorSharpTestDriver.New().SetSourceTextWithCursor(@"
                using System;
                class C { void M() { new DateTime().DayOfWeek =| } }
            ");
            var result = await driver.SendAsync<CompletionsResult>(TypeChar, ' ');
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
            var driver = MirrorSharpTestDriver.New().SetSourceTextWithCursor(@"
                class C {
                    " + methods + @"
                    void T() { M| }
                }
            ");
            var result = await driver.SendAsync<SignaturesResult>(TypeChar, '(');
            Assert.Equal(expected, result.Signatures.Select(s => s.ToString(markSelected: false)));
        }

        [Theory]
        [InlineData("void M(int a, int b, int c) {}", "void C.M(int a, *int b*, int c)")]
        public async Task ExecuteAsync_ProducesSignatureHelpWithSelectedParameter(string methods, string expected) {
            var driver = MirrorSharpTestDriver.New().SetSourceTextWithCursor(@"
                class C {
                    " + methods + @"
                    void T() { M(1| }
                }
            ");
            var result = await driver.SendAsync<SignaturesResult>(TypeChar, ',');
            var signature = result.Signatures.Single();
            Assert.Equal(expected, signature.ToString());
        }

        [Theory]
        [InlineData("void M(int a) {} void M(int a, int b) {}", "void C.M(int a, int b)")]
        public async Task ExecuteAsync_ProducesSignatureHelpWithSelectedSignature(string methods, string expectedSelected) {
            var driver = MirrorSharpTestDriver.New().SetSourceTextWithCursor(@"
                class C {
                    " + methods + @"
                    void T() { M(1| }
                }
            ");
            var result = await driver.SendAsync<SignaturesResult>(TypeChar, ',');
            var selected = result.Signatures.Single(s => s.Selected);
            Assert.Equal(expectedSelected, string.Join("", selected.Parts.Select(p => p.Text)));
        }

        [Fact]
        public async Task ExecuteAsync_ProducesEmptySignatureHelp_OnClosingParenthesis() {
            var driver = MirrorSharpTestDriver.New().SetSourceTextWithCursor(@"
                class C {
                    void M() {}
                    void T() { M| }
                }
            ");
            await driver.SendAsync(TypeChar, '(');
            var result = await driver.SendAsync<SignaturesResult>(TypeChar, ')');
            Assert.Equal(0, result.Signatures.Count);
        }
    }
}
