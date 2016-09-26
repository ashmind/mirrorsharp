using System;
using System.Text;
using Microsoft.CodeAnalysis.Text;
using MirrorSharp.Internal;
using MirrorSharp.Internal.Commands;
using Xunit;

namespace MirrorSharp.Tests {
    public class ReplaceTextHandlerTests : HandlerTestsBase<ReplaceTextHandler> {
        [Theory]
        [InlineData("abc", "0:2:0:x", "xc", 0)]
        [InlineData("abc", "0:0:0:x", "xabc", 0)]
        [InlineData("abc", "0:0:2:", "abc", 2)]
        [InlineData("abc", "3:0:0:x:y", "abcx:y", 0)]
        public async void ExecuteAsync_AddsSpecifiedCharacter(string initialText, string dataString, string expectedText, int expectedCursorPosition) {
            var session = new WorkSession {
                SourceText = SourceText.From(initialText)
            };
            var data = Encoding.UTF8.GetBytes(dataString);

            await ExecuteAsync(session, new ArraySegment<byte>(data));

            Assert.Equal(expectedText, session.SourceText.ToString());
            Assert.Equal(expectedCursorPosition, session.CursorPosition);
        }
    }
}
