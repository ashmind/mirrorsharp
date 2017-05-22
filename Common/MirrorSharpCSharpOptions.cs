using System.Collections.Immutable;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using MirrorSharp.Advanced;

namespace MirrorSharp {
    public class MirrorSharpCSharpOptions : MirrorSharpRoslynOptions<CSharpParseOptions, CSharpCompilationOptions> {
        internal MirrorSharpCSharpOptions() : base(
            new CSharpParseOptions(),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary),
            ImmutableList.Create<MetadataReference>(MetadataReference.CreateFromFile(typeof(object).GetTypeInfo().Assembly.Location))
        ) {
        }
    }
}
