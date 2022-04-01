using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CodeActions;

namespace MirrorSharp.Internal.Roslyn.Internals {
    internal interface ICodeActionInternals {
        bool IsInlinable(CodeAction action);
        ImmutableArray<CodeAction> GetNestedCodeActions(CodeAction action);
    }
}
