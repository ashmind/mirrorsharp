using System.Linq;
using System.Threading.Tasks;
using MirrorSharp.Internal;
using MirrorSharp.Testing;
using MirrorSharp.Testing.Internal.Results;
using MirrorSharp.Testing.Results;
using Xunit;

// ReSharper disable HeapView.ClosureAllocation

namespace MirrorSharp.Tests {
    using static CommandIds;

    public class ApplyDiagnosticActionHandlerTests {
        [Fact]
        public async Task ExecuteAsync_ProducesExpectedChanges_ForMissingNamespace() {
            var driver = MirrorSharpTestDriver.New().SetText(@"class C { Action a; }");
            var action = await ExecuteSlowUpdateAndGetDiagnosticActionAsync(driver, "Action", "using");

            var changes = await driver.SendAsync<ChangesResult>(ApplyDiagnosticAction, action.Id);

            Assert.Equal(
                new[] { new { Start = 0, Length = 0, Text = "using System;\r\n\r\n" } },
                changes.Changes.Select(c => new { c.Start, c.Length, c.Text })
            );
        }

        [Fact]
        public async Task ExecuteAsync_DoesNotModifyCurrentSession() {
            var driver = MirrorSharpTestDriver.New().SetText(@"class C { Action a; }");
            var action = await ExecuteSlowUpdateAndGetDiagnosticActionAsync(driver, "Action", "using");

            var textBefore = driver.Session.Roslyn.SourceText;
            await driver.SendAsync(ApplyDiagnosticAction, action.Id);

            Assert.Same(textBefore, driver.Session.Roslyn.SourceText);
            Assert.Equal(textBefore.ToString(), (await driver.Session.Roslyn.Document.GetTextAsync()).ToString());
            Assert.Same(driver.Session.Roslyn.Workspace.CurrentSolution, driver.Session.Roslyn.Project.Solution);
        }

        private static async Task<SlowUpdateDiagnosticAction> ExecuteSlowUpdateAndGetDiagnosticActionAsync(
            MirrorSharpTestDriver driver, string diagnosticMessageFilter, string actionTitleFilter
        ) {
            var result = await driver.SendSlowUpdateAsync();
            var diagnostic = result.Diagnostics.Single(d => d.Message?.Contains(diagnosticMessageFilter) ?? false);
            return diagnostic.Actions.Single(a => a.Title?.Contains(actionTitleFilter) ?? false);
        }
    }
}
