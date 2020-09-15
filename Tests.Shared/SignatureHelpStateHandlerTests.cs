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
            var driver = MirrorSharpTestDriver.New().SetTextWithCursor(@"
                class C {
                    void M(int a) {}
                    void T() { M(1|) }
                }
            ");
            var result = await driver.SendWithRequiredResultAsync<SignaturesResult>(SignatureHelpState, 'F');
            Assert.Equal(
                new[] { "void C.M(*int a*)" },
                result.Signatures.Select(s => s.ToString())
            );
        }

        [Fact]
        public async Task ExecuteAsync_ProducesExpectedSignatureHelpInfo() {
            var driver = MirrorSharpTestDriver.New(MirrorSharpOptionsWithXmlDocumentation.Instance).SetTextWithCursor(@"
                class C {
                    void T() { 'a'.Equals(|); }
                }
            ");
            var result = await driver.SendWithRequiredResultAsync<SignaturesResult>(SignatureHelpState, 'F');
            var selected = Assert.Single(result.Signatures, s => s.Selected);
            Assert.NotNull(selected.Info);
            Assert.Equal(
                "Returns a value that indicates whether this instance is equal to the specified char object.",
                string.Join("", selected.Info!.Parts)
            );
        }

        [Fact]
        public async Task ExecuteAsync_ProducesExpectedSignatureHelpParameterInfo() {
            var driver = MirrorSharpTestDriver.New(MirrorSharpOptionsWithXmlDocumentation.Instance).SetTextWithCursor(@"
                class C {
                    void T() { 'a'.Equals(|); }
                }
            ");
            var result = await driver.SendWithRequiredResultAsync<SignaturesResult>(SignatureHelpState, 'F');
            var parameter = Assert.Single(result.Signatures, s => s.Selected).Info?.Parameter;
            Assert.NotNull(parameter);
            Assert.Equal("obj", parameter!.Name);
            Assert.Equal("An object to compare to this instance.", string.Join("", parameter.Parts));
        }
    }
}
