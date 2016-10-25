using BenchmarkDotNet.Attributes;
using MirrorSharp.Internal;
using MirrorSharp.Internal.Handlers;
using MirrorSharp.Tests.Internal;

namespace MirrorSharp.Benchmarks {
    public class SignatureHelpBenchmarks {
        private WorkSession _sessionWithHelp;
        private WorkSession _sessionWithNoHelp;

        [Setup]
        public void Setup() {
            _sessionWithHelp = TestHelper.SessionFromTextWithCursor("class C { void M(int a) { M| } }");
            _sessionWithNoHelp = TestHelper.SessionFromTextWithCursor("class C { void M(int a) { M()| } }");
        }

        [Benchmark]
        public void TypeCharExpectingSignatureHelp() {
            TestHelper.ExecuteHandlerAsync<TypeCharHandler>(_sessionWithHelp, '(');
        }

        [Benchmark]
        public void TypeCharNotExpectingSignatureHelp() {
            TestHelper.ExecuteHandlerAsync<TypeCharHandler>(_sessionWithNoHelp, ';');
        }
    }
}
