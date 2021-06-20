using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using MirrorSharp.Internal;
using MirrorSharp.Internal.Handlers;
using MirrorSharp.Internal.Handlers.Shared;
using MirrorSharp.Testing;
using MirrorSharp.Testing.Internal;

namespace MirrorSharp.Benchmarks {
    public class SignatureHelpBenchmarks {
        private static readonly AsyncData LeftParenthesis = new(Encoding.UTF8.GetBytes("("), false, () => Task.FromResult<ReadOnlyMemory<byte>?>(null));
        private static readonly AsyncData Semicolon = new(Encoding.UTF8.GetBytes(";"), false, () => Task.FromResult<ReadOnlyMemory<byte>?>(null));

        private TypeCharHandler? _handler;
        private WorkSession? _sessionWithHelp;
        private WorkSession? _sessionWithNoHelp;

        [IterationSetup]
        public void Setup() {
            _sessionWithHelp = MirrorSharpTestDriver.New().SetTextWithCursor("class C { void M(int a) { M| } }").Session;
            _sessionWithNoHelp = MirrorSharpTestDriver.New().SetTextWithCursor("class C { void M(int a) { M()| } }").Session;
            _handler = new TypeCharHandler(new TypedCharEffects(new CompletionSupport(), new SignatureHelpSupport()));
        }

        [Benchmark]
        public void TypeCharExpectingSignatureHelp() {
            _handler!.ExecuteAsync(LeftParenthesis, _sessionWithHelp!, new StubCommandResultSender(), CancellationToken.None)
                .GetAwaiter().GetResult();
        }

        [Benchmark]
        public void TypeCharNotExpectingSignatureHelp() {
            _handler!.ExecuteAsync(Semicolon, _sessionWithNoHelp!, new StubCommandResultSender(), CancellationToken.None)
                .GetAwaiter().GetResult();
        }
    }
}
