using System.Linq;
using System.Threading.Tasks;
using MirrorSharp.Internal.Commands;
using MirrorSharp.Tests.Internal;
using MirrorSharp.Tests.Internal.Results;
using Xunit;

namespace MirrorSharp.Tests {
    using static TestHelper;

    public class ApplyDiagnosticActionHandlerTests {
        [Fact]
        public async Task ExecuteAsync_CanAddRequiredNamespace() {
            var session = SessionFromTextWithCursor(@"class C { Action a;| }");
            var result = await ExecuteHandlerAsync<SlowUpdateHandler, SlowUpdateResult>(session);
            var diagnostic = result.Diagnostics.Single(d => d.Message.Contains("Action"));
            var action = diagnostic.Actions.Single(a => a.Title.Contains("using"));

            await ExecuteHandlerAsync<ApplyDiagnosticActionHandler>(session, action.Id);

            Assert.Equal(
                "using System;\r\n\r\nclass C { Action a; }",
                session.SourceText.ToString()
            );
        }
    }
}
