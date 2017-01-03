using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using MirrorSharp.Internal;
using MirrorSharp.Testing;
using MirrorSharp.Testing.Results;
using Xunit;

namespace MirrorSharp.Tests {
    using static CommandIds;

    public class SlowUpdateHandlerTests {
        [Fact]
        public async Task SlowUpdate_ProducesDiagnosticWithCustomTagUnnecessary_ForUnusedNamespace() {
            var driver = MirrorSharpTestDriver.New().SetSourceTextWithCursor(@"using System;|");
            var result = await driver.SendAsync<SlowUpdateResult<object>>(SlowUpdate);

            Assert.Contains(
                new { severity = DiagnosticSeverity.Hidden.ToString("G").ToLowerInvariant(), isUnnecessary = true },
                result.Diagnostics.Select(
                    d => new { severity = d.Severity, isUnnecessary = d.Tags.Contains(WellKnownDiagnosticTags.Unnecessary, StringComparer.OrdinalIgnoreCase) }
                ).ToArray()
            );
        }

        [Fact]
        public async Task SlowUpdate_ProducesAllExpectedActions_ForTypeFromUnreferencedNamespace() {
            var driver = MirrorSharpTestDriver.New().SetSourceTextWithCursor(@"class C { Action a;| }");
            var result = await driver.SendAsync<SlowUpdateResult<object>>(SlowUpdate);
            var diagnostic = result.Diagnostics.Single(d => d.Message?.Contains("Action") ?? false);

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
            var driver = MirrorSharpTestDriver.New(languageName: LanguageNames.VisualBasic).SetSourceText(@"
                Class C
                    Sub M()
                    End Sub
                End Class
            ");
            var result = await driver.SendAsync<SlowUpdateResult<object>>(SlowUpdate);

            Assert.NotNull(result);
            Assert.Empty(result.Diagnostics);
        }
    }
}
