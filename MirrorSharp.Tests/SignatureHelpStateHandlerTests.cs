using System.Linq;
using System.Threading.Tasks;
using MirrorSharp.Testing;
using MirrorSharp.Testing.Internal;
using MirrorSharp.Tests.Internal.Results;
using Xunit;

namespace MirrorSharp.Tests {
    using static CommandIds;

    public class SignatureHelpStateHandlerTests {
        [Fact]
        public async Task ExecuteAsync_ProducesExpectedSignatureHelp_WhenForceIsRequested() {
            var test = MirrorSharpTest.StartNew().SetTextWithCursor(@"
                class C {
                    void M(int a) {}
                    void T() { M(1|) }
                }
            ");
            var result = await test.SendAsync<SignaturesResult>(SignatureHelpState, 'F');
            Assert.Equal(
                new[] { "void C.M(*int a*)" },
                result.Signatures.Select(s => s.ToString())
            );
        }
    }
}
