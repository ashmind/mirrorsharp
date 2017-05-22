using System.Collections.Immutable;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.VisualBasic;
using MirrorSharp.Internal.Roslyn;

namespace MirrorSharp.VisualBasic.Internal {
    internal class VisualBasicLanguage : RoslynLanguageBase {
        public VisualBasicLanguage(MirrorSharpVisualBasicOptions options) : base(
            LanguageNames.VisualBasic,
            "Microsoft.CodeAnalysis.VisualBasic.Features",
            "Microsoft.CodeAnalysis.VisualBasic.Workspaces",
            options.ParseOptions ?? new VisualBasicParseOptions(),
            options.CompilationOptions ?? new VisualBasicCompilationOptions(OutputKind.DynamicallyLinkedLibrary),
            options.MetadataReferences ?? ImmutableList.Create<MetadataReference>(MetadataReference.CreateFromFile(typeof(object).GetTypeInfo().Assembly.Location))
        ) {
        }
    }
}
