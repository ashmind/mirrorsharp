using System.Collections.Immutable;
using System.Composition;
using Microsoft.CodeAnalysis.CodeActions;
using MirrorSharp.Internal.Roslyn.Internals;

namespace MirrorSharp.Internal.Roslyn43 {
    [Shared]
    [Export(typeof(ICodeActionInternals))]
    internal class CodeActionInternals : ICodeActionInternals {
        public bool IsInlinable(CodeAction action) {
            Argument.NotNull(nameof(action), action);
            return action.IsInlinable;
        }

        public ImmutableArray<CodeAction> GetNestedCodeActions(CodeAction action) {
            Argument.NotNull(nameof(action), action);
            return action.NestedCodeActions;
        }
    }
}
