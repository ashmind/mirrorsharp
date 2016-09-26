using System;
using System.Collections.Generic;
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

        // ReSharper disable ClassNeverInstantiated.Local
        // ReSharper disable CollectionNeverUpdated.Local
        // ReSharper disable UnusedAutoPropertyAccessor.Local
        private class SlowUpdateResult {
            public IList<ResultDiagnostic> Diagnostics { get; } = new List<ResultDiagnostic>();
        }
        private class ResultDiagnostic {
            public string Severity { get; set; }
            public IList<string> Tags { get; } = new List<string>();
        }
        // ReSharper restore ClassNeverInstantiated.Local
        // ReSharper restore CollectionNeverUpdated.Local
        // ReSharper restore UnusedAutoPropertyAccessor.Local

    }
}
