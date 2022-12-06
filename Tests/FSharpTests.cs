using System.Linq;
using MirrorSharp.FSharp.Internal;
using MirrorSharp.Internal;
using MirrorSharp.Testing;
using MirrorSharp.Testing.Internal.Results;
using Xunit;

// ReSharper disable HeapView.ObjectAllocation
// ReSharper disable HeapView.BoxingAllocation

namespace MirrorSharp.Tests {
    public class FSharpTests {
        private static readonly MirrorSharpOptions Options = new MirrorSharpOptions().EnableFSharp();

        [Fact]
        public async void SlowUpdate_ProducesNoDiagnostics_IfCodeIsValid() {
            var driver = MirrorSharpTestDriver.New(Options, FSharpLanguage.Name);
            var code = @"
                open System

                [<EntryPoint>]
                let main argv =
                    printfn ""Hello World""
                    0
            ".Trim().Replace("                ", "");
            await driver.SendReplaceTextAsync(code);
            var result = await driver.SendSlowUpdateAsync();
            Assert.Equal("", string.Join("\r\n", result.Diagnostics));
        }

        [Fact]
        public async void SlowUpdate_ProducesExpectedDiagnostics_IfCodeHasErrors() {
            var driver = MirrorSharpTestDriver.New(Options, FSharpLanguage.Name);
            await driver.SendReplaceTextAsync("xyz");
            var result = await driver.SendSlowUpdateAsync();

            Assert.Equal(
                new[] { new {
                    Severity = "error",
                    Message = "The value or constructor 'xyz' is not defined.",
                    Span = new { Start = (int?)0, Length = (int?)3 }
                } },
                result.Diagnostics.Select(d => new {
                    d.Severity,
                    d.Message,
                    Span = new { d.Span?.Start, d.Span?.Length }
                }).ToArray()
            );
        }

        [Fact]
        public async void TypeChar_ProducesExpectedCompletion() {
            var driver = MirrorSharpTestDriver.New(Options, FSharpLanguage.Name);
            driver.SetTextWithCursor(@"
                type Test() =
                    member this.Method() = ()

                let test =
                    let t = new Test()
                    t|
            ".Trim().Replace("                ", ""));

            var result = await driver.SendTypeCharAsync('.');

            Assert.NotNull(result);
            Assert.Equal(
                new[] {
                    new { DisplayText = "Equals", Kind = "method" },
                    new { DisplayText = "GetHashCode", Kind = "method" },
                    new { DisplayText = "GetType", Kind = "method" },
                    new { DisplayText = "Method", Kind = "method" },
                    new { DisplayText = "ToString", Kind = "method" },
                },
                // https://github.com/xunit/assert.xunit/pull/36#issuecomment-578990557
                result!.Completions.Select(c => new { c.DisplayText, Kind = c.Kinds.SingleOrDefault() })!
            );
        }

        [Fact]
        public async void ForceCompletion_ProducesExpectedCompletion_ForDelegateTypeInScope() {
            var driver = MirrorSharpTestDriver.New(Options, FSharpLanguage.Name);
            driver.SetTextWithCursor(@"
                type X = obj
                type Y = obj -> obj|
            ".Trim().Replace("                ", ""));

            var result = await driver.SendWithRequiredResultAsync<CompletionsResult>(CommandIds.CompletionState, 'F');

            Assert.NotNull(result);
            Assert.Contains(
                new { DisplayText = "Y", Kind = "delegate" },
                // https://github.com/xunit/assert.xunit/pull/36#issuecomment-578990557
                result!.Completions.Select(c => new { c.DisplayText, Kind = c.Kinds.SingleOrDefault() })!
            );
        }

        [Fact]
        public async void CompletionState_ProducesExpectedCompletionChanges() {
            var driver = MirrorSharpTestDriver.New(Options, FSharpLanguage.Name);
            driver.SetTextWithCursor(@"
                type Test() =
                    member this.Method() = ()

                let test =
                    let t = new Test()
                    t|
            ".Trim().Replace("                ", ""));

            await driver.SendTypeCharAsync('.');
            var changes = await driver.SendWithRequiredResultAsync<ChangesResult>(CommandIds.CompletionState, "3");

            Assert.Equal(
                new[] {
                    new { Start = driver.Session.CursorPosition, Length = 0, Text = "Method" }
                },
                changes.Changes.Select(c => new { c.Start, c.Length, c.Text })
            );
        }

        // Signature help is not supported, but should not crash either
        [Fact]
        public async void SignatureHelpState_ReturnsEmptySignatures_WhenForced() {
            var driver = MirrorSharpTestDriver.New(Options, FSharpLanguage.Name);
            driver.SetTextWithCursor("|");

            var signatures = await driver.SendWithOptionalResultAsync<SignaturesResult>(CommandIds.SignatureHelpState, "F");

            Assert.Null(signatures);
        }
    }
}
