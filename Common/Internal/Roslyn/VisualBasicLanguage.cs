using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.VisualBasic;

namespace MirrorSharp.Internal.Roslyn {
    internal class VisualBasicLanguage : RoslynLanguageBase {
        public VisualBasicLanguage(VisualBasicParseOptions parseOptions, VisualBasicCompilationOptions compilationOptions) : base(
            LanguageNames.VisualBasic,
            "Microsoft.CodeAnalysis.VisualBasic.Features",
            "Microsoft.CodeAnalysis.VisualBasic.Workspaces",
            parseOptions ?? new VisualBasicParseOptions(),
            compilationOptions ?? new VisualBasicCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
        ) {
        }
    }
}
