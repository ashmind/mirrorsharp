using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Completion;

namespace MirrorSharp.Internal.Roslyn {
    internal static class RoslynAssemblies {
        public static readonly Assembly MicrosoftCodeAnalysis = typeof(Compilation).GetTypeInfo().Assembly;
        public static readonly Assembly MicrosoftCodeAnalysisFeatures = typeof(CompletionProvider).GetTypeInfo().Assembly;
        public static readonly Assembly MicrosoftCodeAnalysisWorkspaces = typeof(Workspace).GetTypeInfo().Assembly;
    }
}
