using System;
using Microsoft.FSharp.Collections;
using FSharp.Compiler.CodeAnalysis;
using MirrorSharp.FSharp.Advanced;
using Xunit;
using Range = FSharp.Compiler.Text.Range;
using Microsoft.FSharp.Core;

namespace MirrorSharp.Tests {
    public class FSharpProjectOptionsExtensionsTests {
        [Theory]
        [InlineData(new[] { "--debug+" }, true)]
        [InlineData(new[] { "--debug-" }, false)]
        [InlineData(new string[0], null)]
        public void WithOtherOptionDebug_ReturnsSameInstance_IfValueIsTheSame(string[] otherOptions, bool? newValue) {
            var options = NewOptions(otherOptions: otherOptions);
            var updated = options.WithOtherOptionDebug(newValue);
            Assert.Same(options, updated);
        }

        [Theory]
        [InlineData(new[] { "--debug+" }, false, new[] { "--debug-" })]
        [InlineData(new[] { "--debug+" }, null,  new string[0])]
        [InlineData(new[] { "--debug-" }, true,  new[] { "--debug+" })]
        [InlineData(new[] { "--debug-" }, null,  new string[0])]
        [InlineData(new string[0], true,  new[] { "--debug+" })]
        [InlineData(new string[0], false, new[] { "--debug-" })]
        public void WithOtherOptionDebug_ReturnsExpectedOptions_IfValueIsNotTheSame(string[] otherOptions, bool? newValue, string[] expected) {
            var options = NewOptions(otherOptions: otherOptions).WithOtherOptionDebug(newValue);
            Assert.Equal(expected, options.OtherOptions);
        }

        [Theory]
        [InlineData(new[] { "--optimize+" }, true)]
        [InlineData(new[] { "--optimize-" }, false)]
        [InlineData(new string[0], null)]
        public void WithOtherOptionOptimize_ReturnsSameInstance_IfValueIsTheSame(string[] otherOptions, bool? newValue) {
            var options = NewOptions(otherOptions: otherOptions);
            var updated = options.WithOtherOptionOptimize(newValue);
            Assert.Same(options, updated);
        }

        [Theory]
        [InlineData(new[] { "--optimize+" }, false, new[] { "--optimize-" })]
        [InlineData(new[] { "--optimize+" }, null, new string[0])]
        [InlineData(new[] { "--optimize-" }, true, new[] { "--optimize+" })]
        [InlineData(new[] { "--optimize-" }, null, new string[0])]
        [InlineData(new string[0], true,  new[] { "--optimize+" })]
        [InlineData(new string[0], false, new[] { "--optimize-" })]
        public void WithOtherOptionOptimize_ReturnsExpectedOptions_IfValueIsNotTheSame(string[] otherOptions, bool? newValue, string[] expected) {
            var options = NewOptions(otherOptions: otherOptions).WithOtherOptionOptimize(newValue);
            Assert.Equal(expected, options.OtherOptions);
        }

        [Theory]
        [InlineData(new[] { "--target:exe" }, FSharpTargets.Exe)]
        [InlineData(new[] { "--target:winexe" }, FSharpTargets.WinExe)]
        [InlineData(new[] { "--target:library" }, FSharpTargets.Library)]
        [InlineData(new[] { "--target:module" }, FSharpTargets.Module)]
        [InlineData(new string[0], null)]
        public void WithOtherOptionTarget_ReturnsSameInstance_IfValueIsTheSame(string[] otherOptions, string newValue) {
            var options = NewOptions(otherOptions: otherOptions);
            var updated = options.WithOtherOptionTarget(newValue);
            Assert.Same(options, updated);
        }

        [Theory]
        [InlineData(new[] { "--target:exe" }, FSharpTargets.Library, new[] { "--target:library" })]
        [InlineData(new[] { "--target:library" }, FSharpTargets.Exe, new[] { "--target:exe" })]
        [InlineData(new string[0], FSharpTargets.Library,  new[] { "--target:library" })]
        public void WithOtherOptionTarget_ReturnsExpectedOptions_IfValueIsNotTheSame(string[] otherOptions, string newValue, string[] expected) {
            var options = NewOptions(otherOptions: otherOptions).WithOtherOptionTarget(newValue);
            Assert.Equal(expected, options.OtherOptions);
        }

        [Theory]
        [InlineData(new[] { "--define:TEST" }, true, new[] { "--define:TEST", "--define:DEBUG" })]
        [InlineData(new string[0], true, new[] { "--define:DEBUG" })]
        [InlineData(new[] { "--define:DEBUG" }, false, new string[0])]
        [InlineData(new[] { "--define:TEST", "--define:DEBUG" }, false, new[] { "--define:TEST" })]
        [InlineData(new[] { "--define:TEST" }, false, new[] { "--define:TEST" })]
        public void WithOtherOptionDefine_ReturnsExpectedOptions_IfThereAreChanges(string[] otherOptions, bool defined, string[] expected) {
            var options = NewOptions(otherOptions: otherOptions).WithOtherOptionDefine("DEBUG", defined);
            Assert.Equal(expected, options.OtherOptions);
        }

        [Theory]
        [InlineData(new[] { "--define:DEBUG" }, true)]
        [InlineData(new[] { "--define:TEST", "--define:DEBUG" }, true)]
        [InlineData(new string[0], false)]
        public void WithOtherOptionDefine_ReturnsSameInstance_IfThereAreNoChanges(string[] otherOptions, bool defined) {
            var options = NewOptions(otherOptions: otherOptions);
            var updated = options.WithOtherOptionDefine("DEBUG", defined);
            Assert.Same(options, updated);
        }

        private FSharpProjectOptions NewOptions(string[]? otherOptions = null) {
            return new FSharpProjectOptions(
                "_",
                FSharpOption<string>.None,
                new string[0],
                otherOptions,
                Array.Empty<FSharpReferencedProject>(),
                false,
                false,
                DateTime.MinValue,
                FSharpOption<FSharpUnresolvedReferencesSet>.None,
                FSharpList<Tuple<Range, string, string>>.Empty,
                FSharpOption<long>.None
            );
        }
    }
}