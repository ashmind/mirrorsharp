using MirrorSharp.Testing;
using Xunit;
using System.Threading.Tasks;
using MirrorSharp.Internal;
using MirrorSharp.Testing.Results;

namespace MirrorSharp.Tests.Features {
    public class StartupFailedTests {
        [Fact]
        public async Task AnyCommand_IfStartupFailed_ReturnsStartupError() {
            // Arrange
            var driver = MirrorSharpTestDriver.New(new MirrorSharpOptions().DisableCSharp());

            // Act
            var result = await driver.SendWithRequiredResultAsync<ErrorResult>(CommandIds.SetOptions, "");

            // Assert
            Assert.Contains("???", result.Message);
        }
    }
}
