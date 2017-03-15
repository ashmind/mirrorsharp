using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace MirrorSharp.Internal.Languages {
    internal class CSharpLanguage : LanguageBase {
        public CSharpLanguage() : base(
            LanguageNames.CSharp,
            "Microsoft.CodeAnalysis.CSharp.Features",
            "Microsoft.CodeAnalysis.CSharp.Workspaces",
            new CSharpParseOptions(),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
        ) {
        }
    }
}
