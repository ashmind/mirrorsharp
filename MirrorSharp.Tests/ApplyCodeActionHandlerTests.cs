using System.Linq;
using System.Threading.Tasks;
using MirrorSharp.Internal.Commands;
using MirrorSharp.Tests.Internal;
using Xunit;

namespace MirrorSharp.Tests {
    using static TestHelper;

    public class ApplyCodeActionHandlerTests : HandlerTestsBase<ApplyDiagnosticActionHandler> {
        [Fact]
        public async Task ApplyCodeAction_CanAddRequiredNamespace() {
            var session = SessionFromTextWithCursor(@"class C { Action a;| }");
            var result = await ExecuteAndCaptureResultAsync<SlowUpdateResult>(new SlowUpdateHandler(), session);
            var diagnostic = result.Diagnostics.Single(d => d.Message.Contains("Action"));
            var action = diagnostic.Actions.Single(a => a.Title.Contains("using"));
            await ExecuteAsync(session, ToByteArraySegment(action.Id));

            Assert.Equal(
                "using System;\r\n\r\nclass C { Action a; }",
                session.SourceText.ToString()
            );
        }
    }
}
