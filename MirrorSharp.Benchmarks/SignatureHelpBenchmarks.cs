using System;
using System.Text;
using System.Threading;
using BenchmarkDotNet.Attributes;
using MirrorSharp.Internal;
using MirrorSharp.Internal.Handlers;
using MirrorSharp.Internal.Handlers.Shared;
using MirrorSharp.Testing;
using MirrorSharp.Testing.Internal;

namespace MirrorSharp.Benchmarks {
    public class SignatureHelpBenchmarks {
        private static readonly ArraySegment<byte> LeftParenthesis = new ArraySegment<byte>(Encoding.UTF8.GetBytes("("));
        private static readonly ArraySegment<byte> Semicolon = new ArraySegment<byte>(Encoding.UTF8.GetBytes(";"));

        private TypeCharHandler _handler;
        private WorkSession _sessionWithHelp;
        private WorkSession _sessionWithNoHelp;

        [Setup]
        public void Setup() {
            _sessionWithHelp = MirrorSharpTestDriver.New().SetTextWithCursor("class C { void M(int a) { M| } }").Session;
            _sessionWithNoHelp = MirrorSharpTestDriver.New().SetTextWithCursor("class C { void M(int a) { M()| } }").Session;
            _handler = new TypeCharHandler(new TypedCharEffects(new CompletionSupport(), new SignatureHelpSupport()));
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
