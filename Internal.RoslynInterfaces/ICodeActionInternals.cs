using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CodeActions;

namespace MirrorSharp.Internal.RoslynInterfaces {
    internal interface ICodeActionInternals {
        bool IsInlinable(CodeAction action);
        ImmutableArray<CodeAction> GetNestedCodeActions(CodeAction action);
    }
}
