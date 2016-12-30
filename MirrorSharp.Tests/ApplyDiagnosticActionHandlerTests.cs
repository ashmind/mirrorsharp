using System.Linq;
using System.Threading.Tasks;
using MirrorSharp.Internal;
using MirrorSharp.Internal.Handlers;
using MirrorSharp.Tests.Internal;
using MirrorSharp.Tests.Internal.Results;
using Xunit;

namespace MirrorSharp.Tests {
    using static TestHelper;

    public class ApplyDiagnosticActionHandlerTests {
        [Fact]
        public async Task ExecuteAsync_ProducesExpectedChanges_ForMissingNamespace() {
            var session = SessionFromText(@"class C { Action a; }");
            var action = await ExecuteSlowUpdateAndGetDiagnosticActionAsync(session, "Action", "using");

            var changes = await ExecuteHandlerAsync<ApplyDiagnosticActionHandler, ChangesResult>(session, action.Id);

            Assert.Equal(
                new[] { new { Start = 0, Length = 0, Text = "using System;\r\n\r\n" } },
                changes.Changes.Select(c => new { c.Start, c.Length, c.Text })
            );
        }

        [Fact]
        public async Task ExecuteAsync_DoesNotModifyCurrentSession() {
            var session = SessionFromText(@"class C { Action a; }");
            var action = await ExecuteSlowUpdateAndGetDiagnosticActionAsync(session, "Action", "using");

            var textBefore = session.SourceText;
            await ExecuteHandlerAsync<ApplyDiagnosticActionHandler>(session, action.Id);

            Assert.Same(textBefore, session.SourceText);
            Assert.Equal(textBefore.ToString(), (await session.Document.GetTextAsync()).ToString());
            Assert.Same(session.Workspace.CurrentSolution, session.Project.Solution);
        }

        private static async Task<SlowUpdateResult.ResultAction> ExecuteSlowUpdateAndGetDiagnosticActionAsync(
            WorkSession session, string diagnosticMessageFilter, string actionTitleFilter
        ) {
            var result = await ExecuteHandlerAsync<SlowUpdateHandler, SlowUpdateResult>(session);
            var diagnostic = result.Diagnostics.Single(d => d.Message.Contains(diagnosticMessageFilter));
            return diagnostic.Actions.Single(a => a.Title.Contains(actionTitleFilter));
        }
    }
}
