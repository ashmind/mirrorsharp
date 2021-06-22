using System;
using System.Buffers;
using System.Collections.Immutable;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Net.WebSockets.Mocks;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MirrorSharp.Internal;
using MirrorSharp.Internal.Handlers;
using MirrorSharp.Internal.Handlers.Mocks;
using MirrorSharp.Testing;
using Xunit;
using System.IO;
using SourceMock.Internal;

// ReSharper disable HeapView.ClosureAllocation

namespace MirrorSharp.Tests {
    public class ConnectionTests {
        [Fact]
        public async Task ReceiveAndProcessAsync_CallsMatchingCommand() {
            var socketMock = MockWebSocketToReceive("X123");

            var session = MirrorSharpTestDriver.New().Session;
            var handler = MockCommandHandler('X');
            var connection = new Connection(socketMock, session, CreateCommandHandlers(handler), ArrayPool<byte>.Shared);
            var cancellationToken = new CancellationTokenSource().Token;

            await connection.ReceiveAndProcessAsync(cancellationToken);

            var call = Assert.Single(handler.Calls.ExecuteAsync());
            Assert.Equal("123", Encoding.UTF8.GetString(call.data.GetFirst().Span));
            Assert.Equal(session, call.session);
            Assert.Equal(connection, call.sender);
            Assert.Equal(cancellationToken, call.cancellationToken);
        }

        [Fact]
        public async Task ReceiveAndProcessAsync_HandlesLongMessage() {
            var longArgument = GenerateLongString(10000);
            var socketMock = MockWebSocketToReceive("X" + longArgument);
            var session = MirrorSharpTestDriver.New().Session;

            var segments = new List<ReadOnlyMemory<byte>>();
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
            Assert.Equal(longArgument, string.Join("", segments.Select(s => Encoding.UTF8.GetString(s.ToArray()))));
        }

        private ReadOnlyMemory<T> Copy<T>(ReadOnlyMemory<T> segment) {
            var newArray = new T[segment.Length];
            segment.CopyTo(newArray);
            return newArray;
        }

        private string GenerateLongString(int length) {
            var chars = new char[length];
            for (var i = 0; i < chars.Length; i++) {
                chars[i] = (char)('A' + (i % ('Z' - 'A' + 1)));
            }
            return new string(chars);
        }

        private CommandHandlerMock MockCommandHandler(char commandId, Func<AsyncData, Task>? execute = null) {
            var handler = new CommandHandlerMock();
            handler.Setup.CommandId.Returns(commandId);
            handler.Setup.ExecuteAsync().Runs((data, ss, sn, t) => execute?.Invoke(data) ?? Task.CompletedTask);
            return handler;
        }

        private ImmutableArray<ICommandHandler> CreateCommandHandlers(ICommandHandler handler) {
            var handlers = new ICommandHandler[26];
            handlers[handler.CommandId - 'A'] = handler;
            return ImmutableArray.CreateRange(handlers);
        }

        private static WebSocket MockWebSocketToReceive(string command) {
            var mock = new WebSocketMock();
            var dataStream = new MemoryStream(Encoding.UTF8.GetBytes(command));
            mock.Setup.ReceiveAsync(default(MockArgumentMatcher<ArraySegment<byte>>)).Runs((ArraySegment<byte> data, CancellationToken _) => {
                var count = dataStream.Read(data.Array!, data.Offset, data.Count);
                return Task.FromResult(new WebSocketReceiveResult(count, WebSocketMessageType.Text, dataStream.Position == dataStream.Length));
            });
            return mock;
        }
    }
}