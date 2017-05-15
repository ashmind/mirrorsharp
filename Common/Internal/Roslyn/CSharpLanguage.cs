using System.Collections.Immutable;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace MirrorSharp.Internal.Roslyn {
    internal class CSharpLanguage : RoslynLanguageBase {
        public CSharpLanguage(CSharpParseOptions parseOptions, CSharpCompilationOptions compilationOptions, ImmutableList<MetadataReference> metadataReferences) : base(
            LanguageNames.CSharp,
            "Microsoft.CodeAnalysis.CSharp.Features",
            "Microsoft.CodeAnalysis.CSharp.Workspaces",
            parseOptions ?? new CSharpParseOptions(),
            compilationOptions ?? new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary),
            metadataReferences ?? ImmutableList.Create<MetadataReference>(MetadataReference.CreateFromFile(typeof(object).GetTypeInfo().Assembly.Location))
        ) {
        }
    }
}
