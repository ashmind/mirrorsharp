using System.Linq;
using Microsoft.CodeAnalysis;
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
                    Console.ReadLine() |> ignore
                    0
            ".Trim().Replace("                ", "");
            await driver.SendReplaceTextAsync(code);
            var result = await driver.SendSlowUpdateAsync();
            Assert.Empty(result.Diagnostics);
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

        [Theory]
        [InlineData(OptimizationLevel.Debug)]
        [InlineData(OptimizationLevel.Release)]
        public async void SetOptions_DoesNotCauseAnyDiagnosticIssues_WithEitherOptimizationLevel(OptimizationLevel level) {
            var driver = MirrorSharpTestDriver.New(Options, FSharpLanguage.Name);
            await driver.SendSetOptionAsync("optimize", level.ToString());
            await driver.SendReplaceTextAsync("1 |> ignore");
            var result = await driver.SendSlowUpdateAsync();

            Assert.Empty(result.Diagnostics);
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
                    new { DisplayText = "Equals", Tag = "method" },
                    new { DisplayText = "GetHashCode", Tag = "method" },
                    new { DisplayText = "GetType", Tag = "method" },
                    new { DisplayText = "Method", Tag = "method" },
                    new { DisplayText = "ToString", Tag = "method" },
                },
                result.Completions.Select(c => new { c.DisplayText, Tag = c.Tags.SingleOrDefault() })
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
            var changes = await driver.SendAsync<ChangesResult>(CommandIds.CompletionState, "3");

            Assert.Equal(
                new[] {
                    new { Start = driver.Session.CursorPosition, Length = 0, Text = "Method" }
                },
                changes.Changes.Select(c => new { c.Start, c.Length, c.Text })
            );
        }
    }
}
