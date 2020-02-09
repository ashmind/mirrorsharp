using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Microsoft.CodeAnalysis.QuickInfo;
using MirrorSharp.Internal;
using MirrorSharp.Testing;
using MirrorSharp.Testing.Internal;

namespace MirrorSharp.Tests {
    public partial class RequestInfoTipHandlerTests {
        [Theory]
        [InlineData(
            "class ➭C {}",
            "class C", new[] { "class", "internal" }
        )]
        [InlineData(
            "using System.Threading.Tasks; class C { Task ➭A() {} }",
            // https://github.com/dotnet/roslyn/commit/d9be31dadff3d7a7c30ec38cae5d75bf0f6a0a22
            #if PRE_ROSLYN_36
            "(awaitable) Task C.A()\r\n\r\nUsage:\r\n  await A();",
            #else
            "(awaitable) Task C.A()",
            #endif
            new[] { "method", "private" }
        )]
        [InlineData(
            "class C { string P { g➭et; set; } }",
            "string C.P.get", new[] { "method", "private" }
        )]
        public async Task ExecuteAsync_ProducesExpectedInfoTip(string textWithCursor, string expectedResultText, string[] expectedKinds) {
            var text = TextWithCursor.Parse(textWithCursor, '➭');
            var driver = MirrorSharpTestDriver.New().SetText(text.Text);

            var result = await driver.SendRequestInfoTipAsync(text.CursorPosition);

            Assert.Equal(expectedKinds, result.Kinds);
            Assert.Equal(expectedResultText, result?.ToString());
        }

        [Fact]
        public async Task ExecuteAsync_IncludesXmlDocCommentsInResult() {
            var text = TextWithCursor.Parse("class C { string M(int a) { return a.To➭String(); } }", '➭');
            var driver = MirrorSharpTestDriver.New(MirrorSharpOptionsWithXmlDocumentation.Instance)
                .SetText(text.Text);

            var result = await driver.SendRequestInfoTipAsync(text.CursorPosition);

            var documentation = Assert.Single(result.Sections.Where(e => e.Kind == QuickInfoSectionKinds.DocumentationComments.ToLowerInvariant()));
            Assert.Equal(
                "Converts the numeric value of this instance to its equivalent string representation.",
                documentation.ToString()
            );
        }

        [Fact]
        public async Task ExecuteAsync_DoesNotSendMessage_WhenNoQuickInfo() {
            var driver = MirrorSharpTestDriver.New().SetText("class C {}");

            var result = await driver.SendRequestInfoTipAsync(10);

            Assert.Null(result);
        }
    }
}
