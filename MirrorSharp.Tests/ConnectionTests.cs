using System;
using System.Collections.Immutable;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MirrorSharp.Internal;
using MirrorSharp.Internal.Handlers;
using MirrorSharp.Internal.Languages;
using Moq;
using Xunit;

namespace MirrorSharp.Tests {
    public class ConnectionTests {
        [Fact]
        public async void ReceiveAndProcessAsync_CallsMatchingCommand() {
            var socketMock = Mock.Of<WebSocket>();
            SetupReceive(socketMock, "X");

            var session = new WorkSession(new CSharpLanguage());
            // ReSharper disable once PossibleUnintendedReferenceComparison
            var handler = Mock.Of<ICommandHandler>(h => h.CommandIds == ImmutableList.Create('X'));
            var connection = new Connection(socketMock, session, CreateCommandHandlers(handler));
            var cancellationToken = new CancellationTokenSource().Token;

            await connection.ReceiveAndProcessAsync(cancellationToken);
            Mock.Get(handler).Verify(s => s.ExecuteAsync(It.IsAny<ArraySegment<byte>>(), session, connection, cancellationToken));
        }

        private ImmutableArray<ICommandHandler> CreateCommandHandlers(ICommandHandler handler) {
            var handlers = new ICommandHandler[26];
            handlers[handler.CommandIds[0] - 'A'] = handler;
            return ImmutableArray.CreateRange(handlers);
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
