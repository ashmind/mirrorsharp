using System;
using System.Threading.Tasks;
using MirrorSharp.Advanced;
using MirrorSharp.Testing;
using Xunit;

namespace MirrorSharp.Tests.Features {
    // This can't be tested for VB until https://github.com/dotnet/roslyn/issues/9063
    public class RoslynScriptModeTests {
        // anything from System would do, as I don't want to require tests to add references
        private static readonly Type TestHostType = typeof(Random);
        private const string TestHostMethodName = nameof(Random.Next);

        [Fact]
        public async Task Script_ProducesNoErrors_WhenSetInInitialOptions() {
            var options = new MirrorSharpOptions()
                .SetupCSharp(o => o.SetScriptMode(hostObjectType: TestHostType));
            var driver = MirrorSharpTestDriver.New(options)
                .SetText($"var x = {TestHostMethodName}();");

            var result = await driver.SendSlowUpdateAsync();

            Assert.Equal("", result.JoinErrors());
        }

        [Fact]
        public async Task Script_ProducesNoErrors_WhenSetThroughOptionExtension() {
            var extensions = new MirrorSharpServices {
                SetOptionsFromClient = new ScriptModeExtension(TestHostType)
            };
            var driver = MirrorSharpTestDriver.New(extensions)
                .SetText($"var x = {TestHostMethodName}();");

            await driver.SendSetOptionAsync("x-mode", "script");
            var result = await driver.SendSlowUpdateAsync();

            Assert.Equal("", result.JoinErrors());
        }

        [Fact]
        public async Task Script_CanApplyTextChanges_WhenSetThroughOptionExtension() {
            var extensions = new MirrorSharpServices {
                SetOptionsFromClient = new ScriptModeExtension(TestHostType)
            };
            var driver = MirrorSharpTestDriver.New(extensions);

            await driver.SendSetOptionAsync("x-mode", "script");
            await driver.SendReplaceTextAsync($"var x = {TestHostMethodName}();");
            var result = await driver.SendSlowUpdateAsync();

            Assert.Equal("", result.JoinErrors());
        }

        private class ScriptModeExtension : ISetOptionsFromClientExtension {
            private readonly Type _hostObjectType;

            public ScriptModeExtension(Type hostObjectType) {
                _hostObjectType = hostObjectType;
            }

            public bool TrySetOption(IWorkSession session, string name, string value) {
                if (name != "x-mode" || value != "script")
                    return false;

                session.Roslyn.SetScriptMode(hostObjectType: _hostObjectType);
                return true;
            }
        }
    }
}
