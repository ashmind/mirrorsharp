using System.Collections.Immutable;
using System.Composition;
using Microsoft.CodeAnalysis.CodeActions;
using MirrorSharp.Internal.Roslyn.Internals;

namespace MirrorSharp.Internal.Roslyn41 {
    [Shared]
    [Export(typeof(ICodeActionInternals))]
    internal class CodeActionInternals : ICodeActionInternals {
        public bool IsInlinable(CodeAction action) {
            Argument.NotNull(nameof(action), action);
            return action.IsInlinable;
        }

        public Roslyn.Internals.CodeActionPriority GetPriority(CodeAction action) {
            Argument.NotNull(nameof(action), action);
            return (Roslyn.Internals.CodeActionPriority)(int)action.Priority;
        }

        public ImmutableArray<CodeAction> GetNestedCodeActions(CodeAction action) {
            Argument.NotNull(nameof(action), action);
            return action.NestedCodeActions;
        }
    }
}
