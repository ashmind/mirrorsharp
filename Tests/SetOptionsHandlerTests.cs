using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using MirrorSharp.Advanced;
using MirrorSharp.Advanced.Mocks;
using MirrorSharp.Internal;
using MirrorSharp.Testing;
using MirrorSharp.Testing.Results;
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
            var extensionMock = new SetOptionsFromClientExtensionMock();
            extensionMock.Setup.TrySetOption().Returns(true);

            var driver = MirrorSharpTestDriver.New(new MirrorSharpServices { SetOptionsFromClient = extensionMock });
            await driver.SendAsync(SetOptions, "x-testkey=testvalue");

            Assert.Equal(
                (driver.Session, "x-testkey", "testvalue"),
                Assert.Single(extensionMock.Calls.TrySetOption())
            );
        }

        [Fact]
        public async void ExecuteAsync_ReappliesExtensionOption_WhenChangingLanguage() {
            var extensionMock = new SetOptionsFromClientExtensionMock();
            extensionMock.Setup.TrySetOption().Returns(true);

            var driver = MirrorSharpTestDriver.New(new MirrorSharpOptions().EnableVisualBasic(), new MirrorSharpServices {
                SetOptionsFromClient = extensionMock
            });

            await driver.SendAsync(SetOptions, "x-testkey=testvalue");
            var previousCallCount = extensionMock.Calls.TrySetOption().Count;
            await driver.SendAsync(SetOptions, "language=" + LanguageNames.VisualBasic);

            Assert.Equal((driver.Session, "x-testkey", "testvalue"), Assert.Single(
                extensionMock.Calls.TrySetOption().Skip(previousCallCount)
            ));
        }

        [Fact]
        public async void ExecuteAsync_DoesNotApplyExtensionOptionTwice_WhenChangingLanguage_IfOptionIsSentWithLanguageChange() {
            var extensionMock = new SetOptionsFromClientExtensionMock();
            extensionMock.Setup.TrySetOption().Returns(true);

            var driver = MirrorSharpTestDriver.New(new MirrorSharpOptions().EnableVisualBasic(), new MirrorSharpServices {
                SetOptionsFromClient = extensionMock
            });

            await driver.SendAsync(SetOptions, "x-testkey=testvalue");
            var previousCallCount = extensionMock.Calls.TrySetOption().Count;
            await driver.SendAsync(SetOptions, "language=" + LanguageNames.VisualBasic + ",x-testkey=testvalue");

            Assert.Equal((driver.Session, "x-testkey", "testvalue"), Assert.Single(
                extensionMock.Calls.TrySetOption().Skip(previousCallCount)
            ));
        }

        [Fact]
        public async void ExecuteAsync_EchoesOptionsIncludingPreviousCalls() {
            var extensionMock = new SetOptionsFromClientExtensionMock();
            extensionMock.Setup.TrySetOption().Returns(true);

            var driver = MirrorSharpTestDriver.New(new MirrorSharpServices { SetOptionsFromClient = extensionMock });
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
