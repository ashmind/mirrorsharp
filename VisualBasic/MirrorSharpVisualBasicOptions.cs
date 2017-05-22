using System.Collections.Immutable;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.VisualBasic;
using MirrorSharp.Advanced;

namespace MirrorSharp.VisualBasic {
    public class MirrorSharpVisualBasicOptions : MirrorSharpRoslynOptions<VisualBasicParseOptions, VisualBasicCompilationOptions> {
        internal MirrorSharpVisualBasicOptions() : base(
            new VisualBasicParseOptions(),
            new VisualBasicCompilationOptions(OutputKind.DynamicallyLinkedLibrary),
            ImmutableList.Create<MetadataReference>(MetadataReference.CreateFromFile(typeof(object).GetTypeInfo().Assembly.Location))
        ) {
        }
    }
}
