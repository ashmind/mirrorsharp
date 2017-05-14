using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using MirrorSharp.Testing;
using Xunit;

namespace MirrorSharp.Tests {
    public class FSharpTests {
        [Fact]
        public async void SlowUpdate_ProducesNoDiagnostics_IfCodeIsValid() {
            var driver = MirrorSharpTestDriver.New();
            await driver.SendSetOptionAsync("language", "F#");
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
            var driver = MirrorSharpTestDriver.New();
            await driver.SendSetOptionAsync("language", "F#");
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
        public async void SetOptions_DoesNotProduceAnyDiagnosticIssues_WithEitherOptimizationLevel(OptimizationLevel level) {
            var driver = MirrorSharpTestDriver.New();
            await driver.SendSetOptionsAsync(new Dictionary<string, string> {
                { "language", "F#" },
                { "optimize", level.ToString() }
            });
            await driver.SendReplaceTextAsync("1 |> ignore");
            var result = await driver.SendSlowUpdateAsync();

            Assert.Empty(result.Diagnostics);
        }
    }
}
