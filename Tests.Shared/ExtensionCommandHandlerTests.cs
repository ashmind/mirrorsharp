using System.Buffers;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using MirrorSharp.Internal;
using MirrorSharp.Internal.Results;
using MirrorSharp.Testing;
using Xunit;

namespace MirrorSharp.Tests {
    using static CommandIds;

    public class ExtensionCommandHandlerTests {
        [Fact]
        public async void ExecuteAsync_ExecutesExtensionCommand() {
            var capturedDataList = new List<AsyncData>();
            var extensionMock = new Mock<ICommandExtension>();
            extensionMock.SetupGet(x => x.Name).Returns("test");
            extensionMock.Setup(x => x.ExecuteAsync(
                Capture.In(capturedDataList),
                It.IsAny<WorkSession>(),
                It.IsAny<ICommandResultSender>(),
                It.IsAny<CancellationToken>()
            )).Returns(Task.CompletedTask);

            var driver = MirrorSharpTestDriver.New(new MirrorSharpOptions {
                Extensions = { extensionMock.Object }
            });
            await driver.SendAsync(ExtensionCommand, new[] { "test:a1", "a2", "a3" });

            var data = Assert.Single(capturedDataList);
            var dataString = await AsyncDataConvert.ToUtf8StringAsync(data, 0, ArrayPool<char>.Shared);
            Assert.Equal("a1a2a3", dataString);
        }
    }
}
