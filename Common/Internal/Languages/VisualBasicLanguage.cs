using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.VisualBasic;

namespace MirrorSharp.Internal.Languages {
    internal class VisualBasicLanguage : LanguageBase {
        public VisualBasicLanguage() : base(
            LanguageNames.VisualBasic,
            "Microsoft.CodeAnalysis.VisualBasic.Features",
            "Microsoft.CodeAnalysis.VisualBasic.Workspaces",
            new VisualBasicParseOptions(),
            new VisualBasicCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
        ) {
        }
    }
}
