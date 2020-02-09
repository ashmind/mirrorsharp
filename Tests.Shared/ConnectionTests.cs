using System;
using System.Buffers;
using System.Collections.Immutable;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MirrorSharp.Internal;
using MirrorSharp.Internal.Handlers;
using MirrorSharp.Internal.Results;
using MirrorSharp.Testing;
using Moq;
using Xunit;
using System.IO;

// ReSharper disable HeapView.ClosureAllocation

namespace MirrorSharp.Tests {
    public class ConnectionTests {
        [Fact]
        public async Task ReceiveAndProcessAsync_CallsMatchingCommand() {
            var socketMock = MockWebSocketToReceive("X");

            var session = MirrorSharpTestDriver.New().Session;
            var handler = MockCommandHandler('X');
            var connection = new Connection(socketMock, session, CreateCommandHandlers(handler), ArrayPool<byte>.Shared);
            var cancellationToken = new CancellationTokenSource().Token;

            await connection.ReceiveAndProcessAsync(cancellationToken);
            Mock.Get(handler).Verify(s => s.ExecuteAsync(It.IsAny<AsyncData>(), session, connection, cancellationToken));
        }
        
        [Fact]
        public async Task ReceiveAndProcessAsync_HandlesLongMessage() {
            var longArgument = GenerateLongString(10000);
            var socketMock = MockWebSocketToReceive("X" + longArgument);
            var session = MirrorSharpTestDriver.New().Session;

            var segments = new List<ArraySegment<byte>>();
            var handler = MockCommandHandler('X', async data => {
                segments.Add(Copy(data.GetFirst()));
                var next = await data.GetNextAsync();
                while (next != null) {
                    segments.Add(Copy(next.Value));
                    next = await data.GetNextAsync();
                }
            });
            var connection = new Connection(socketMock, session, CreateCommandHandlers(handler), ArrayPool<byte>.Shared);

            await connection.ReceiveAndProcessAsync(CancellationToken.None);
            Assert.Equal(longArgument, string.Join("", segments.Select(s => Encoding.UTF8.GetString(s))));
        }

        private ArraySegment<T> Copy<T>(ArraySegment<T> segment) {
            var newArray = new T[segment.Array!.Length];
            Buffer.BlockCopy(segment.Array, 0, newArray, 0, segment.Array.Length);
            return new ArraySegment<T>(newArray, segment.Offset, segment.Count);
        }

        private string GenerateLongString(int length) {
            var chars = new char[length];
            for (var i = 0; i < chars.Length; i++) {
                chars[i] = (char)('A' + (i % ('Z' - 'A' + 1)));
            }
            return new string(chars);
        }

        private ICommandHandler MockCommandHandler(char commandId, Func<AsyncData, Task>? execute = null) {
            var handler = Mock.Of<ICommandHandler>(h => h.CommandId == commandId);
            Mock.Get(handler)
                .Setup(h => h.ExecuteAsync(It.IsAny<AsyncData>(), It.IsAny<WorkSession>(), It.IsAny<ICommandResultSender>(), It.IsAny<CancellationToken>()))
                .Returns((AsyncData data, WorkSession s, ICommandResultSender sender, CancellationToken token) => execute?.Invoke(data) ?? Task.CompletedTask);
            return handler;
        }

        private ImmutableArray<ICommandHandler> CreateCommandHandlers(ICommandHandler handler) {
            var handlers = new ICommandHandler[26];
            handlers[handler.CommandId - 'A'] = handler;
            return ImmutableArray.CreateRange(handlers);
        }

        private static WebSocket MockWebSocketToReceive(string command) {
            var mock = new Mock<WebSocket>();
            var dataStream = new MemoryStream(Encoding.UTF8.GetBytes(command));
            mock.Setup(m => m.ReceiveAsync(It.IsAny<ArraySegment<byte>>(), It.IsAny<CancellationToken>()))
                .Returns((ArraySegment<byte> data, CancellationToken _) => {
                    var count = dataStream.Read(data.Array!, data.Offset, data.Count);
                    return Task.FromResult(new WebSocketReceiveResult(count, WebSocketMessageType.Text, dataStream.Position == dataStream.Length));
                });
            return mock.Object;
        }
    }
}
