using System;
using System.Text;
using MirrorSharp.Internal;
using MirrorSharp.Internal.Commands;
using Xunit;

namespace MirrorSharp.Tests {
    public class MoveCursorHandlerTests : HandlerTestsBase<MoveCursorHandler> {
        [Theory]
        [InlineData("1", 1)]
        [InlineData("79", 79)]
        [InlineData("1234567890", 1234567890)]
        public async void ExecuteAsync_UpdatesSessionCursorPosition(string dataString, int expectedPosition) {
            var session = new WorkSession();
            var data = Encoding.UTF8.GetBytes(dataString);
            await ExecuteAsync(session, new ArraySegment<byte>(data));
            Assert.Equal(expectedPosition, session.CursorPosition);
        }
    }
}
