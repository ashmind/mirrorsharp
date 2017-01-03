using System.Linq;
using System.Threading.Tasks;
using MirrorSharp.Internal;
using MirrorSharp.Testing;
using MirrorSharp.Testing.Internal.Results;
using Xunit;

namespace MirrorSharp.Tests {
    using static CommandIds;

    public class SignatureHelpStateHandlerTests {
        [Fact]
        public async Task ExecuteAsync_ProducesExpectedSignatureHelp_WhenForceIsRequested() {
            var driver = MirrorSharpTestDriver.New().SetSourceTextWithCursor(@"
                class C {
                    void M(int a) {}
                    void T() { M(1|) }
                }
            ");
            var result = await driver.SendAsync<SignaturesResult>(SignatureHelpState, 'F');
            Assert.Equal(
                new[] { "void C.M(*int a*)" },
                result.Signatures.Select(s => s.ToString())
            );
        }
    }
}
