using Microsoft.CodeAnalysis;
using MirrorSharp.Internal.Handlers;
using MirrorSharp.Tests.Internal;
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
    }
}
