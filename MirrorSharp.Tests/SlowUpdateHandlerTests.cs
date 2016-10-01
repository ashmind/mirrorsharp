using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using MirrorSharp.Internal.Commands;
using MirrorSharp.Tests.Internal;
using Xunit;

namespace MirrorSharp.Tests {
    using static TestHelper;

    public class SlowUpdateHandlerTests : HandlerTestsBase<SlowUpdateHandler> {
        [Fact]
        public async Task SlowUpdate_ProducesDiagnosticWithCustomTagUnnecessary_ForUnusedNamespace() {
            var session = SessionFromTextWithCursor(@"using System;|");
            var result = await ExecuteAndCaptureResultAsync<SlowUpdateResult>(session);

            Assert.Contains(
                new { severity = DiagnosticSeverity.Hidden.ToString("G").ToLowerInvariant(), isUnnecessary = true },
                result.Diagnostics.Select(
                    d => new { severity = d.Severity, isUnnecessary = d.Tags.Contains(WellKnownDiagnosticTags.Unnecessary, StringComparer.OrdinalIgnoreCase) }
                ).ToArray()
            );
        }

        [Fact]
        public async Task SlowUpdate_ProducesAllExpectedActions_ForTypeFromUnreferencedNamespace() {
            var session = SessionFromTextWithCursor(@"class C { Action a;| }");
            var result = await ExecuteAndCaptureResultAsync<SlowUpdateResult>(session);
            var diagnostic = result.Diagnostics.Single(d => d.Message.Contains("Action"));

            Assert.Equal(
                new[] { "System.Action", "using System;" },
                diagnostic.Actions.Select(a => a.Title).OrderBy(t => t).ToArray()
            );
        }
    }
}
