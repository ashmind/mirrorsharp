using System.Threading.Tasks;
using Xunit;
using MirrorSharp.Internal;
using MirrorSharp.Testing;
using MirrorSharp.Testing.Internal.Results;

namespace MirrorSharp.Tests {
    using static CommandIds;

    public class RequestInfoTipHandlerTests {
        [Theory]
        [InlineData(
            "class C {}", 7,
            "class C", new[] { "class", "internal" }
        )]
        [InlineData(
            "using System.Threading.Tasks; class C { Task A() {} }", 46,
            "(awaitable) Task C.A()\r\n\r\nUsage:\r\n  await A();", new[] { "method", "private" }
        )]
        [InlineData(
            "class C { string P { get; set; } }", 22,
            "string C.P.get", new[] { "method", "private" }
        )]
        public async Task ExecuteAsync_ProducesExpectedInfoTip(string text, int position, string expectedResultText, string[] expectedKinds) {
            var driver = MirrorSharpTestDriver.New().SetText(text);

            var result = await driver.SendAsync<InfoTipResult>(RequestInfoTip, $"N{position}");

            Assert.Equal(expectedKinds, result.Kinds);
            Assert.Equal(expectedResultText, result?.ToString());
        }

        [Fact]
        public async Task ExecuteAsync_DoesNotSendMessage_WhenNoQuickInfoAndStateIsInactive() {
            var driver = MirrorSharpTestDriver.New().SetText("class C {}");

            var result = await driver.SendAsync<InfoTipResult>(RequestInfoTip, "N10");

            Assert.Null(result);
        }

        [Fact]
        public async Task ExecuteAsync_SendsEmptyMessage_WhenNoQuickInfoButStateIsActive() {
            var driver = MirrorSharpTestDriver.New().SetText("class C {}");

            var result = await driver.SendAsync<InfoTipResult>(RequestInfoTip, "A10");

            Assert.NotNull(result);
            Assert.Empty(result.Kinds);
            Assert.Empty(result.Entries);
        }
    }
}
