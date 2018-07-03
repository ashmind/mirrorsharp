using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Completion;
#if QUICKINFO
using Microsoft.CodeAnalysis.Editor.Peek;
#endif

namespace MirrorSharp.Internal.Reflection {
    internal static class RoslynAssemblies {
        public static readonly Assembly MicrosoftCodeAnalysisFeatures = typeof(CompletionProvider).GetTypeInfo().Assembly;
        public static readonly Assembly MicrosoftCodeAnalysisWorkspaces = typeof(Workspace).GetTypeInfo().Assembly;
        #if QUICKINFO
        public static readonly Assembly MicrosoftCodeAnalysisEditorFeatures = typeof(IPeekableItemFactory).GetTypeInfo().Assembly;
        #endif
    }
}
