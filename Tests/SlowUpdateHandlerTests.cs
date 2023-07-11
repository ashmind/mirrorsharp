using System;
using System.Collections.Immutable;
using System.Linq;
using System.Mocks;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Xunit;
using MirrorSharp.Advanced.Mocks;
using MirrorSharp.Internal;
using MirrorSharp.Testing;
using MirrorSharp.Testing.Results;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.CSharp;

// ReSharper disable HeapView.BoxingAllocation

namespace MirrorSharp.Tests {
    using static CommandIds;

    public class SlowUpdateHandlerTests {
        [Fact]
        public async Task SlowUpdate_ProducesDiagnosticWithCustomTagUnnecessary_ForUnusedNamespace() {
            var driver = MirrorSharpTestDriver.New().SetTextWithCursor(@"using System;|");

            var result = await driver.SendWithRequiredResultAsync<SlowUpdateResult<object>>(SlowUpdate);

            Assert.Contains(
                new { severity = DiagnosticSeverity.Hidden.ToString("G").ToLowerInvariant(), isUnnecessary = true },
                result.Diagnostics.Select(
                    d => new { severity = d.Severity, isUnnecessary = d.Tags.Contains(WellKnownDiagnosticTags.Unnecessary, StringComparer.OrdinalIgnoreCase) }
                ).ToArray()
            );
        }

        [Fact]
        public async Task SlowUpdate_ProducesAllExpectedActions_ForTypeFromUnreferencedNamespace() {
            var driver = MirrorSharpTestDriver.New().SetTextWithCursor(@"class C { Action a;| }");

            var result = await driver.SendWithRequiredResultAsync<SlowUpdateResult<object>>(SlowUpdate);

            var diagnostic = result.Diagnostics.Single(d => d.Message?.Contains("Action") ?? false);
            Assert.Equal(
                new[] {
                    "using System;",
                    "Generate class 'Action'",
                    "Generate nested class 'Action'",
                    "System.Action"
                },
                diagnostic.Actions.Select(a => a.Title).ToArray()
            );
        }


        [Fact]
        public async Task SlowUpdate_Succeeds_ForValidVisualBasicCode() {
            var driver = MirrorSharpTestDriver.New(new MirrorSharpOptions().EnableVisualBasic(), languageName: LanguageNames.VisualBasic).SetText(@"
                Public Class C
                    Public Sub M()
                    End Sub
                End Class
            ".Replace("                ", "").Trim());

            var result = await driver.SendWithRequiredResultAsync<SlowUpdateResult<object>>(SlowUpdate);

            Assert.Empty(result.Diagnostics);
        }

        [Fact]
        public async Task SlowUpdate_DisposesExtensionResult_IfDisposable() {
            var disposable = new DisposableMock();
            var slowUpdate = new SlowUpdateExtensionMock();
            slowUpdate.Setup.ProcessAsync().ReturnsAsync(disposable);
            var driver = MirrorSharpTestDriver.New(new MirrorSharpServices {
                SlowUpdate = slowUpdate
            });
            await driver.SendAsync(SlowUpdate);

            Assert.Single(disposable.Calls.Dispose());
        }

        [Fact]
        public async Task SlowUpdate_ProducesDiagnostic_FromCustomAnalyzerInstance() {
            var reference = new AnalyzerImageReference(ImmutableArray.Create<DiagnosticAnalyzer>(new TestAnalyzer()));
            var driver = MirrorSharpTestDriver.New(
                new MirrorSharpOptions().SetupCSharp(c => c.AnalyzerReferences = c.AnalyzerReferences.Add(reference))
            ).SetText("class C {}");

            var result = await driver.SendWithRequiredResultAsync<SlowUpdateResult<object>>(SlowUpdate);

            Assert.Contains(
                ("T01", "Test"),
                result.Diagnostics.Select(d => (d.Id, d.Message)).ToArray()
            );
        }

        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        private class TestAnalyzer : DiagnosticAnalyzer {
            #pragma warning disable RS2008 // Enable analyzer release tracking
            private static readonly DiagnosticDescriptor Descriptor = new ("T01", "Test", "Test", "Test", DiagnosticSeverity.Warning, isEnabledByDefault: true);
            #pragma warning restore RS2008

            public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Descriptor);

            public override void Initialize(AnalysisContext context) {
                context.EnableConcurrentExecution();
                context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
                context.RegisterSyntaxNodeAction(
                    c => c.ReportDiagnostic(Diagnostic.Create(Descriptor, c.Node.GetLocation())),
                    SyntaxKind.ClassDeclaration
                );
            }
        }
    }
}
