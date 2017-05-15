using System.Collections.Immutable;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.VisualBasic;

namespace MirrorSharp.Internal.Roslyn {
    internal class VisualBasicLanguage : RoslynLanguageBase {
        public VisualBasicLanguage(VisualBasicParseOptions parseOptions, VisualBasicCompilationOptions compilationOptions, ImmutableList<MetadataReference> metadataReferences) : base(
            LanguageNames.VisualBasic,
            "Microsoft.CodeAnalysis.VisualBasic.Features",
            "Microsoft.CodeAnalysis.VisualBasic.Workspaces",
            parseOptions ?? new VisualBasicParseOptions(),
            compilationOptions ?? new VisualBasicCompilationOptions(OutputKind.DynamicallyLinkedLibrary),
            metadataReferences ?? ImmutableList.Create<MetadataReference>(MetadataReference.CreateFromFile(typeof(object).GetTypeInfo().Assembly.Location))
        ) {
        }
    }
}
