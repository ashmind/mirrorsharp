using System;
using System.Text;
using System.Threading;
using BenchmarkDotNet.Attributes;
using MirrorSharp.Internal;
using MirrorSharp.Internal.Handlers;
using MirrorSharp.Internal.Handlers.Shared;
using MirrorSharp.Tests.Internal;

namespace MirrorSharp.Benchmarks {
    public class SignatureHelpBenchmarks {
        private static readonly ArraySegment<byte> LeftParenthesis = new ArraySegment<byte>(Encoding.UTF8.GetBytes("("));
        private static readonly ArraySegment<byte> Semicolon = new ArraySegment<byte>(Encoding.UTF8.GetBytes(";"));

        private TypeCharHandler _handler;
        private WorkSession _sessionWithHelp;
        private WorkSession _sessionWithNoHelp;

        [Setup]
        public void Setup() {
            _sessionWithHelp = TestHelper.SessionFromTextWithCursor("class C { void M(int a) { M| } }");
            _sessionWithNoHelp = TestHelper.SessionFromTextWithCursor("class C { void M(int a) { M()| } }");
            _handler = new TypeCharHandler(new CompletionSupport(), new SignatureHelpSupport());
        }

        [Benchmark]
        public void TypeCharExpectingSignatureHelp() {
            _handler.ExecuteAsync(LeftParenthesis, _sessionWithHelp, new StubCommandResultSender(), CancellationToken.None)
                .GetAwaiter().GetResult();
        }

        [Benchmark]
        public void TypeCharNotExpectingSignatureHelp() {
            _handler.ExecuteAsync(Semicolon, _sessionWithNoHelp, new StubCommandResultSender(), CancellationToken.None)
                .GetAwaiter().GetResult();
        }
    }
}
