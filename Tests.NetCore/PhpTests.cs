using System.Linq;
using Microsoft.CodeAnalysis;
using MirrorSharp.Php.Internal;
using MirrorSharp.Internal;
using MirrorSharp.Testing;
using MirrorSharp.Testing.Internal.Results;
using Xunit;

// ReSharper disable HeapView.ObjectAllocation
// ReSharper disable HeapView.BoxingAllocation

namespace MirrorSharp.Tests {
    public class PhpTests {
        private static readonly MirrorSharpOptions Options = new MirrorSharpOptions().EnablePhp();

        [Fact]
        public async void SlowUpdate_ProducesNoDiagnostics_IfCodeIsValid() {
            var driver = MirrorSharpTestDriver.New(Options, PhpLanguage.Name);
            var code = @"
                <?php

                function main() {
                    echo ""Hello World!"";
                }

                main();
            ".Trim().Replace("                ", "");
            await driver.SendReplaceTextAsync(code);
            var result = await driver.SendSlowUpdateAsync();
            Assert.Empty(result.Diagnostics);
        }

        [Fact]
        public async void SlowUpdate_ProducesExpectedDiagnostics_IfCodeHasErrors() {
            var driver = MirrorSharpTestDriver.New(Options, PhpLanguage.Name);
            await driver.SendReplaceTextAsync("<?php new A;");
            var result = await driver.SendSlowUpdateAsync();
            
            Assert.Equal(
                new[] { new {
                    Severity = "warning",
                    Message = "Class 'A' not found",
                    Span = new { Start = (int?)10, Length = (int?)1 }
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