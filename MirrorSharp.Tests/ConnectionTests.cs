using System;
using System.Collections.Immutable;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Completion;
using Microsoft.CodeAnalysis.Text;
using MirrorSharp.Internal;
using Moq;
using Xunit;

namespace MirrorSharp.Tests {
    public class ConnectionTests {
        private static readonly CompletionChange NoCompletionChange = CompletionChange.Create(ImmutableArray<TextChange>.Empty);

        [Theory]
        [InlineData("C1", 1)]
        [InlineData("C79", 79)]
        [InlineData("C1234567890", 1234567890)]
        public async void ReceiveAndProcessAsync_CallsMoveCursorOnSession_AfterReceivingMoveCursorCommand(string command, int expectedPosition) {
            var socketMock = Mock.Of<WebSocket>();
            SetupReceive(socketMock, command);
            var sessionMock = Mock.Of<IWorkSession>();

            await new Connection(socketMock, sessionMock).ReceiveAndProcessAsync();
            Mock.Get(sessionMock).Verify(s => s.MoveCursor(expectedPosition));
        }

        [Theory]
        [InlineData("T1", '1')]
        [InlineData("Ta", 'a')]
        [InlineData("T\u0216", '\u0216')]
        [InlineData("T月", '月')]
        public async void ReceiveAndProcessAsync_CallsTypeCharAsyncOnSession_AfterReceivingTypeCharCommand(string command, char expectedChar) {
            var socketMock = Mock.Of<WebSocket>();
            SetupReceive(socketMock, command);
            var sessionMock = Mock.Of<IWorkSession>();

            await new Connection(socketMock, sessionMock).ReceiveAndProcessAsync();
            Mock.Get(sessionMock).Verify(s => s.TypeCharAsync(expectedChar));
        }

        [Theory]
        [InlineData("R1:10:1:text", 1, 10, 1, "text")]
        [InlineData("R1:10:1:", 1, 10, 1, "")]
        [InlineData("R1:1:1:t:e:xt", 1, 1, 1, "t:e:xt")]
        public async void ReceiveAndProcessAsync_CallsReplaceTextOnSession_AfterReceivingReplaceCommand(string command, int expectedStart, int expectedLength, int expectedPosition, string expectedText) {
            var socketMock = Mock.Of<WebSocket>();
            SetupReceive(socketMock, command);
            var sessionMock = Mock.Of<IWorkSession>();

            await new Connection(socketMock, sessionMock).ReceiveAndProcessAsync();
            Mock.Get(sessionMock).Verify(s => s.ReplaceText(expectedStart, expectedLength, expectedText, expectedPosition));
        }

        [Theory]
        [InlineData("S1", 1)]
        [InlineData("S79", 79)]
        [InlineData("S1234567890", 1234567890)]
        public async void ReceiveAndProcessAsync_CallsGetCompletionChangeAsyncOnSession_AfterReceivingCommitCompletionCommand(string command, int expectedItemIndex) {
            var socketMock = Mock.Of<WebSocket>();
            SetupReceive(socketMock, command);
            var sessionMock = Mock.Of<IWorkSession>(
                s => s.GetCompletionChangeAsync(It.IsAny<int>()) == Task.FromResult(NoCompletionChange)
            );

            await new Connection(socketMock, sessionMock).ReceiveAndProcessAsync();
            Mock.Get(sessionMock).Verify(s => s.GetCompletionChangeAsync(expectedItemIndex));
        }

        private static void SetupReceive(WebSocket socket, string command) {
            Mock.Get(socket)
                .Setup(m => m.ReceiveAsync(It.IsAny<ArraySegment<byte>>(), It.IsAny<CancellationToken>()))
                .Returns((ArraySegment<byte> s, CancellationToken _) => {
                    var byteCount = Encoding.UTF8.GetBytes(command.ToCharArray(), 0, command.Length, s.Array, s.Offset);
                    return Task.FromResult(new WebSocketReceiveResult(byteCount, WebSocketMessageType.Text, true));
                });
        }
    }
}
