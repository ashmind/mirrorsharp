using System.Collections.Generic;
using MirrorSharp.Internal;
using MirrorSharp.Testing;
using Xunit;

namespace MirrorSharp.Tests {
    using static CommandIds;

    public class FSharpTests {
        [Fact]
        public async void Prototype() {
            var driver = MirrorSharpTestDriver.New();
            await driver.SendSetOptionsAsync(new Dictionary<string, string> {{ "language", "F#" }});
            var code = @"open System

[<EntryPoint>]
let main argv = 
    printfn ""Hello World""
    Console.ReadLine() |> ignore
    0";
            await driver.SendAsync(ReplaceText, $"0:0:0::{code}");
            var result = await driver.SendSlowUpdateAsync();
            Assert.Empty(result.Diagnostics);
        }
    }
}
