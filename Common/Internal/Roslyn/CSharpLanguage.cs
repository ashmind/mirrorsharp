using System.Collections.Immutable;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace MirrorSharp.Internal.Roslyn {
    internal class CSharpLanguage : RoslynLanguageBase {
        public CSharpLanguage(MirrorSharpCSharpOptions options) : base(
            LanguageNames.CSharp,
            "Microsoft.CodeAnalysis.CSharp.Features",
            "Microsoft.CodeAnalysis.CSharp.Workspaces",
            options.ParseOptions ?? new CSharpParseOptions(),
            options.CompilationOptions ?? new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary),
            options.MetadataReferences ?? ImmutableList.Create<MetadataReference>(MetadataReference.CreateFromFile(typeof(object).GetTypeInfo().Assembly.Location))
        ) {
        }
    }
}
