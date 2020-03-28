using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using MirrorSharp.Advanced;
using MirrorSharp.Internal;
using MirrorSharp.Testing;
using MirrorSharp.Testing.Results;
using Moq;
using Xunit;

// ReSharper disable HeapView.ClosureAllocation

namespace MirrorSharp.Tests {
    using static CommandIds;

    public class SetOptionsHandlerTests {
        [Theory]
        [InlineData(LanguageNames.CSharp)]
        [InlineData(LanguageNames.VisualBasic)]
        public async void ExecuteAsync_UpdatesSessionLanguage(string languageName) {
            var driver = MirrorSharpTestDriver.New(new MirrorSharpOptions().EnableVisualBasic());
            await driver.SendAsync(SetOptions, "language=" + languageName);
            Assert.Equal(languageName, driver.Session.Language.Name);
        }

        [Fact]
        public async void ExecuteAsync_CallsSetOptionExtension_IfOptionHasExtensionPrefix() {
            var extensionMock = new Mock<ISetOptionsFromClientExtension>();
            extensionMock.SetReturnsDefault(true);

            var driver = MirrorSharpTestDriver.New(new MirrorSharpServices { SetOptionsFromClient = extensionMock.Object });
            await driver.SendAsync(SetOptions, "x-testkey=testvalue");
            extensionMock.Verify(x => x.TrySetOption(driver.Session, "x-testkey", "testvalue"));
        }

        [Fact]
        public async void ExecuteAsync_EchoesOptionsIncludingPreviousCalls() {
            var extensionMock = new Mock<ISetOptionsFromClientExtension>();
            extensionMock.SetReturnsDefault(true);

            var driver = MirrorSharpTestDriver.New(new MirrorSharpServices { SetOptionsFromClient = extensionMock.Object });
            await driver.SendAsync(SetOptions, "x-key1=value1");
            var optionsEcho = await driver.SendWithRequiredResultAsync<OptionsEchoResult>(SetOptions, "x-key2=value2");
            Assert.Equal(
                new Dictionary<string, string> {
                    ["x-key1"] = "value1",
                    ["x-key2"] = "value2"
                },
                optionsEcho.Options
            );
        }
    }
}
