using Microsoft.CodeAnalysis;
using MirrorSharp.Internal.Roslyn;

namespace MirrorSharp.VisualBasic.Internal {
    internal class VisualBasicLanguage : RoslynLanguageBase {
        public VisualBasicLanguage(MirrorSharpVisualBasicOptions options) : base(
            LanguageNames.VisualBasic,
            "Microsoft.CodeAnalysis.VisualBasic.Features",
            "Microsoft.CodeAnalysis.VisualBasic.Workspaces",
            "Microsoft.CodeAnalysis.VisualBasic.EditorFeatures",
            options
        ) {
        }
    }
}
