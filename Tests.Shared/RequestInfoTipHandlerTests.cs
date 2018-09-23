using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.QuickInfo;
using MirrorSharp.Internal;
using MirrorSharp.Testing;
using MirrorSharp.Testing.Internal;
using MirrorSharp.Testing.Internal.Results;

namespace MirrorSharp.Tests {
    using static CommandIds;

    public partial class RequestInfoTipHandlerTests {
        private static readonly MirrorSharpOptions MirrorSharpOptionsWithXmlDocumentation = new MirrorSharpOptions().SetupCSharp(c => {
            c.MetadataReferences = ImmutableList<MetadataReference>.Empty;
            c.AddMetadataReferencesFromFiles(MscorlibReferenceAssemblyPath);
        });

        [Theory]
        [InlineData(
            "class ➭C {}",
            "class C", new[] { "class", "internal" }
        )]
        [InlineData(
            "using System.Threading.Tasks; class C { Task ➭A() {} }",
            "(awaitable) Task C.A()\r\n\r\nUsage:\r\n  await A();", new[] { "method", "private" }
        )]
        [InlineData(
            "class C { string P { g➭et; set; } }",
            "string C.P.get", new[] { "method", "private" }
        )]
        public async Task ExecuteAsync_ProducesExpectedInfoTip(string textWithCursor, string expectedResultText, string[] expectedKinds) {
            var text = TextWithCursor.Parse(textWithCursor, '➭');
            var driver = MirrorSharpTestDriver.New().SetText(text.Text);

            var result = await driver.SendAsync<InfoTipResult>(RequestInfoTip, $"{text.CursorPosition}");

            Assert.Equal(expectedKinds, result.Kinds);
            Assert.Equal(expectedResultText, result?.ToString());
        }

        [Fact]
        public async Task ExecuteAsync_IncludesXmlDocCommentsInResult() {
            var text = TextWithCursor.Parse("class C { string M(int a) { return a.To➭String(); } }", '➭');
            var driver = MirrorSharpTestDriver.New(MirrorSharpOptionsWithXmlDocumentation)
                .SetText(text.Text);

            var result = await driver.SendAsync<InfoTipResult>(RequestInfoTip, $"{text.CursorPosition}");

            var documentation = Assert.Single(result.Entries.Where(e => e.Kind == QuickInfoSectionKinds.DocumentationComments.ToLowerInvariant()));
            Assert.Equal(
                "Converts the numeric value of this instance to its equivalent string representation.",
                documentation.ToString()
            );
        }

        [Fact]
        public async Task ExecuteAsync_DoesNotSendMessage_WhenNoQuickInfo() {
            var driver = MirrorSharpTestDriver.New().SetText("class C {}");

            var result = await driver.SendAsync<InfoTipResult>(RequestInfoTip, "10");

            Assert.Null(result);
        }
    }
}
