using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using MirrorSharp.Testing;
using MirrorSharp.Testing.Internal;
using MirrorSharp.Tests.Internal.Results;
using Xunit;

namespace MirrorSharp.Tests {
    using static CommandIds;

    public class SlowUpdateHandlerTests {
        [Fact]
        public async Task SlowUpdate_ProducesDiagnosticWithCustomTagUnnecessary_ForUnusedNamespace() {
            var test = MirrorSharpTest.StartNew().SetTextWithCursor(@"using System;|");
            var result = await test.SendAsync<SlowUpdateResult>(SlowUpdate);

            Assert.Contains(
                new { severity = DiagnosticSeverity.Hidden.ToString("G").ToLowerInvariant(), isUnnecessary = true },
                result.Diagnostics.Select(
                    d => new { severity = d.Severity, isUnnecessary = d.Tags.Contains(WellKnownDiagnosticTags.Unnecessary, StringComparer.OrdinalIgnoreCase) }
                ).ToArray()
            );
        }

        [Fact]
        public async Task SlowUpdate_ProducesAllExpectedActions_ForTypeFromUnreferencedNamespace() {
            var test = MirrorSharpTest.StartNew().SetTextWithCursor(@"class C { Action a;| }");
            var result = await test.SendAsync<SlowUpdateResult>(SlowUpdate);
            var diagnostic = result.Diagnostics.Single(d => d.Message.Contains("Action"));

            Assert.Equal(
                new[] {
                    "Generate class 'Action'",
                    "Generate nested class 'Action'",
                    "System.Action",
                    "using System;"
                },
                diagnostic.Actions.Select(a => a.Title).OrderBy(t => t).ToArray()
            );
        }

        [Fact]
        public async Task SlowUpdate_Succeeds_ForValidVisualBasicCode() {
            var test = MirrorSharpTest.StartNew(languageName: LanguageNames.VisualBasic).SetText(@"
                Class C
                    Sub M()
                    End Sub
                End Class
            ");
            var result = await test.SendAsync<SlowUpdateResult>(SlowUpdate);

            Assert.NotNull(result);
            Assert.Empty(result.Diagnostics);
        }
    }
}
