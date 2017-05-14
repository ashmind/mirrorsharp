using System.Collections.Generic;
using System.Linq;
using MirrorSharp.Internal;
using MirrorSharp.Testing;
using Xunit;

namespace MirrorSharp.Tests {
    using static CommandIds;

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
    }
}
