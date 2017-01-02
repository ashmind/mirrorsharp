using System.Linq;
using System.Threading.Tasks;
using MirrorSharp.Testing;
using MirrorSharp.Testing.Internal;
using MirrorSharp.Tests.Internal.Results;
using Xunit;

namespace MirrorSharp.Tests {
    using static CommandIds;

    public class ApplyDiagnosticActionHandlerTests {
        [Fact]
        public async Task ExecuteAsync_ProducesExpectedChanges_ForMissingNamespace() {
            var test = MirrorSharpTest.StartNew().SetText(@"class C { Action a; }");
            var action = await ExecuteSlowUpdateAndGetDiagnosticActionAsync(test, "Action", "using");

            var changes = await test.SendAsync<ChangesResult>(ApplyDiagnosticAction, action.Id);

            Assert.Equal(
                new[] { new { Start = 0, Length = 0, Text = "using System;\r\n\r\n" } },
                changes.Changes.Select(c => new { c.Start, c.Length, c.Text })
            );
        }

        [Fact]
        public async Task ExecuteAsync_DoesNotModifyCurrentSession() {
            var test = MirrorSharpTest.StartNew().SetText(@"class C { Action a; }");
            var action = await ExecuteSlowUpdateAndGetDiagnosticActionAsync(test, "Action", "using");

            var textBefore = test.Session.SourceText;
            await test.SendAsync(ApplyDiagnosticAction, action.Id);

            Assert.Same(textBefore, test.Session.SourceText);
            Assert.Equal(textBefore.ToString(), (await test.Session.Document.GetTextAsync()).ToString());
            Assert.Same(test.Session.Workspace.CurrentSolution, test.Session.Project.Solution);
        }

        private static async Task<SlowUpdateResult.ResultAction> ExecuteSlowUpdateAndGetDiagnosticActionAsync(
            MirrorSharpTest test, string diagnosticMessageFilter, string actionTitleFilter
        ) {
            var result = await test.SendAsync<SlowUpdateResult>(SlowUpdate);
            var diagnostic = result.Diagnostics.Single(d => d.Message.Contains(diagnosticMessageFilter));
            return diagnostic.Actions.Single(a => a.Title.Contains(actionTitleFilter));
        }
    }
}
