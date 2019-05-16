using FSharp.Compiler.SourceCodeServices;

namespace MirrorSharp.FSharp.Advanced {
    /// <summary>
    /// Provides a list of constants for <see cref="FSharpProjectOptions.OtherOptions" /> <c>--target</c> option.
    /// </summary>
    public static class FSharpTargets {
        /// <summary>Corresponds to <c>--target:exe</c>.</summary>
        public const string Exe = "exe";

        /// <summary>Corresponds to <c>--target:winexe</c>.</summary>
        public const string WinExe = "winexe";

        /// <summary>Corresponds to <c>--target:library</c>.</summary>
        public const string Library = "library";

        /// <summary>Corresponds to <c>--target:module</c>.</summary>
        public const string Module = "module";
    }
}
