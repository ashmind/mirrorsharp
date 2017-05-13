using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace MirrorSharp.Internal.Roslyn {
    internal class CSharpLanguage : RoslynLanguageBase {
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
