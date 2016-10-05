using System.Text;
using MirrorSharp.Internal;
using MirrorSharp.Internal.Commands;
using MirrorSharp.Tests.Internal;
using Xunit;

namespace MirrorSharp.Tests {
    using static TestHelper;

    public class MoveCursorHandlerTests {
        [Theory]
        [InlineData("1", 1)]
        [InlineData("79", 79)]
        [InlineData("1234567890", 1234567890)]
        public async void ExecuteAsync_UpdatesSessionCursorPosition(string dataString, int expectedPosition) {
            var session = new WorkSession();
            var data = Encoding.UTF8.GetBytes(dataString);
            await ExecuteHandlerAsync<MoveCursorHandler>(session, data);
            Assert.Equal(expectedPosition, session.CursorPosition);
        }
    }
}
