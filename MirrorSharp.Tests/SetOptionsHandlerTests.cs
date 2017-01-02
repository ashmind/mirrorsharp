using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using MirrorSharp.Advanced;
using MirrorSharp.Testing;
using MirrorSharp.Testing.Internal;
using MirrorSharp.Tests.Internal.Results;
using Moq;
using Xunit;

namespace MirrorSharp.Tests {
    using static CommandIds;

    public class SetOptionsHandlerTests {
        [Theory]
        [InlineData(LanguageNames.CSharp)]
        [InlineData(LanguageNames.VisualBasic)]
        public async void ExecuteAsync_UpdatesSessionLanguage(string languageName) {
            var test = MirrorSharpTest.StartNew();
            await test.SendAsync(SetOptions, "language=" + languageName);
            Assert.Equal(languageName, test.Session.Language.Name);
        }

        [Theory]
        [InlineData("debug",   OptimizationLevel.Debug)]
        [InlineData("release", OptimizationLevel.Release)]
        public async void ExecuteAsync_UpdatesSessionCompilationOptimizationsLevel(string value, OptimizationLevel expectedLevel) {
            var test = MirrorSharpTest.StartNew();
            await test.SendAsync(SetOptions, "optimize=" + value);
            Assert.Equal(expectedLevel, test.Session.Project.CompilationOptions.OptimizationLevel);
        }

        [Fact]
        public async void ExecuteAsync_PreservesSessionWorkspace_WhenUpdatingOptimizeToTheSameValue() {
            var test = MirrorSharpTest.StartNew().SetText("test");
            test.Session.ChangeCompilationOptions(nameof(CompilationOptions.OptimizationLevel), c => c.WithOptimizationLevel(OptimizationLevel.Release));
            var workspace = test.Session.Workspace;
            await test.SendAsync(SetOptions, "optimize=release");
            Assert.Same(workspace, test.Session.Workspace);
        }

        [Fact]
        public async void ExecuteAsync_PreservesSessionSourceText_WhenUpdatingOptions() {
            var test = MirrorSharpTest.StartNew().SetText("test");
            await test.SendAsync(SetOptions, "optimize=debug");
            Assert.Equal("test", test.Session.SourceText.ToString());
        }

        [Fact]
        public async void ExecuteAsync_PreservesSessionCursorPosition_WhenUpdatingOptions() {
            var test = MirrorSharpTest.StartNew().SetTextWithCursor("test|");
            await test.SendAsync(SetOptions, "optimize=debug");
            Assert.Equal(4, test.Session.CursorPosition);
        }

        [Fact]
        public async void ExecuteAsync_CallsSetOptionExtension_IfOptionHasExtensionPrefix() {
            var extensionMock = new Mock<ISetOptionsFromClientExtension>();
            extensionMock.SetReturnsDefault(true);

            var test = MirrorSharpTest.StartNew(new MirrorSharpOptions { SetOptionsFromClient = extensionMock.Object });
            await test.SendAsync(SetOptions, "x-testkey=testvalue");
            extensionMock.Verify(x => x.TrySetOption(test.Session, "x-testkey", "testvalue"));
        }

        [Fact]
        public async void ExecuteAsync_EchoesOptionsIncludingPreviousCalls() {
            var extensionMock = new Mock<ISetOptionsFromClientExtension>();
            extensionMock.SetReturnsDefault(true);

            var test = MirrorSharpTest.StartNew(new MirrorSharpOptions { SetOptionsFromClient = extensionMock.Object });
            await test.SendAsync(SetOptions, "optimize=release,x-key1=value1");
            var optionsEcho = await test.SendAsync<OptionsEchoResult>(SetOptions, "x-key2=value2");
            Assert.Equal(
                new Dictionary<string, string> {
                    ["optimize"] = "release",
                    ["x-key1"] = "value1",
                    ["x-key2"] = "value2"
                },
                optionsEcho.Options
            );
        }
    }
}
