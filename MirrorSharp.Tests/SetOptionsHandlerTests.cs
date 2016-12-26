using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using MirrorSharp.Advanced;
using MirrorSharp.Internal.Handlers;
using MirrorSharp.Tests.Internal;
using Moq;
using Xunit;

namespace MirrorSharp.Tests {
    using static TestHelper;

    public class SetOptionsHandlerTests {
        [Theory]
        [InlineData(LanguageNames.CSharp)] // this is a noop at the moment, but VB is not implemented yet
        public async void ExecuteAsync_UpdatesSessionLanguage(string languageName) {
            var session = Session();
            await ExecuteHandlerAsync<SetOptionsHandler>(session, "language=" + languageName);
            Assert.Equal(languageName, session.Language.Name);
        }

        [Theory]
        [InlineData("debug",   OptimizationLevel.Debug)]
        [InlineData("release", OptimizationLevel.Release)]
        public async void ExecuteAsync_UpdatesSessionCompilationOptimizationsLevel(string value, OptimizationLevel expectedLevel) {
            var session = Session();
            await ExecuteHandlerAsync<SetOptionsHandler>(session, "optimize=" + value);
            Assert.Equal(expectedLevel, session.Project.CompilationOptions.OptimizationLevel);
        }

        [Fact]
        public async void ExecuteAsync_PreservesSessionWorkspace_WhenUpdatingOptimizeToTheSameValue() {
            var session = SessionFromText("test");
            session.ChangeCompilationOptions(nameof(CompilationOptions.OptimizationLevel), c => c.WithOptimizationLevel(OptimizationLevel.Release));
            var workspace = session.Workspace;
            await ExecuteHandlerAsync<SetOptionsHandler>(session, "optimize=release");
            Assert.Same(workspace, session.Workspace);
        }

        [Fact]
        public async void ExecuteAsync_PreservesSessionSourceText_WhenUpdatingOptions() {
            var session = SessionFromText("test");
            await ExecuteHandlerAsync<SetOptionsHandler>(session, "optimize=debug");
            Assert.Equal("test", session.SourceText.ToString());
        }

        [Fact]
        public async void ExecuteAsync_PreservesSessionCursorPosition_WhenUpdatingOptions() {
            var session = SessionFromTextWithCursor("test|");
            await ExecuteHandlerAsync<SetOptionsHandler>(session, "optimize=debug");
            Assert.Equal(4, session.CursorPosition);
        }

        [Fact]
        public async void ExecuteAsync_CallsSetOptionExtension_IfOptionHasExtensionPrefix() {
            var extensionMock = new Mock<ISetOptionsFromClientExtension>();
            extensionMock.SetReturnsDefault(true);

            var session = Session();
            await ExecuteHandlerAsync<SetOptionsHandler>(
                session, "x-testkey=testvalue", new MirrorSharpOptions { SetOptionsFromClient = extensionMock.Object }
            );
            extensionMock.Verify(x => x.TrySetOption(session, "x-testkey", "testvalue"));
        }
    }
}
