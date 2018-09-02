using System;
using Microsoft.CodeAnalysis;

namespace MirrorSharp.Internal.Roslyn {
    internal static class RoslynScriptHelper {
        public static void Validate(bool isScript, Type hostObjectType) {
            if (!isScript && hostObjectType != null)
                throw new ArgumentException($"HostObjectType requires script mode (IsScript must be true).", nameof(hostObjectType));
        }

        public static SourceCodeKind GetSourceKind(bool isScript) => isScript ? SourceCodeKind.Script : SourceCodeKind.Regular;
    }
}
