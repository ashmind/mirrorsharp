using System;
using System.Collections.Immutable;
using System.Composition;
using Microsoft.CodeAnalysis.CodeActions;
using MirrorSharp.Internal.RoslynInterfaces;

namespace MirrorSharp.Internal.Roslyn36 {
    [Shared]
    [Export(typeof(ICodeActionInternals))]
    internal class CodeActionInternals : ICodeActionInternals {
        public bool IsInlinable(CodeAction action) {
            if (action == null)
                throw new ArgumentNullException(nameof(action));
            return action.IsInlinable;
        }

        public ImmutableArray<CodeAction> GetNestedCodeActions(CodeAction action) {
            if (action == null)
                throw new ArgumentNullException(nameof(action));
            return action.NestedCodeActions;
        }
    }
}
