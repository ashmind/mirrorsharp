using System;
using System.Linq;
using System.Threading.Tasks;
using MirrorSharp.Internal.Handlers;
using MirrorSharp.Tests.Internal;
using MirrorSharp.Tests.Internal.Results;
using Xunit;

namespace MirrorSharp.Tests {
    using static TestHelper;

    public class SignatureHelpStateHandlerTests {
        [Fact]
        public async Task ExecuteAsync_ProducesExpectedSignatureHelp_WhenForceIsRequested() {
            var session = SessionFromTextWithCursor(@"
                class C {
                    void M(int a) {}
                    void T() { M(1|) }
                }
            ");
            var result = await ExecuteHandlerAsync<SignatureHelpStateHandler, SignaturesResult>(session, 'F');
            Assert.Equal(
                new[] { "void C.M(*int a*)" },
                result.Signatures.Select(s => s.ToString())
            );
        }
    }
}
