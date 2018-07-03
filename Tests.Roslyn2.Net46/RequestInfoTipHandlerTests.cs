using System.Threading.Tasks;
using Xunit;
using MirrorSharp.Internal;
using MirrorSharp.Internal.Reflection;
using MirrorSharp.Testing;
using MirrorSharp.Testing.Internal.Results;

namespace MirrorSharp.Tests {
    using static CommandIds;

    public class RequestInfoTipHandlerTests {
        [Fact]
        public async Task ExecuteAsync_ProducesExpectedInfoTip_ForClass() {
            VisualStudioAssemblyStubs.Register();
            var driver = MirrorSharpTestDriver.New().SetText(@"class C {}");

            var result = await driver.SendAsync<InfoTipResult>(RequestInfoTip, 7);

            Assert.NotNull(result);
            Assert.Equal("<tip>", result.ToString());
        }
    }
}
