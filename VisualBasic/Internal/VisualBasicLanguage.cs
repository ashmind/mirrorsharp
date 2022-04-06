using Microsoft.CodeAnalysis;
using MirrorSharp.Internal;
using MirrorSharp.Internal.Roslyn;

namespace MirrorSharp.VisualBasic.Internal {
    internal class VisualBasicLanguage : RoslynLanguageBase {
        public VisualBasicLanguage(LanguageCreationContext context, MirrorSharpVisualBasicOptions options) : base(
            context,
            LanguageNames.VisualBasic,
            "Microsoft.CodeAnalysis.VisualBasic.Features",
            "Microsoft.CodeAnalysis.VisualBasic.Workspaces",
            options
        ) {
        }
    }
}
