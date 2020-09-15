using System.Collections.Immutable;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using MirrorSharp.Advanced;

namespace MirrorSharp {
    /// <summary>MirrorSharp options for C#</summary>
    public class MirrorSharpCSharpOptions : MirrorSharpRoslynOptions<MirrorSharpCSharpOptions, CSharpParseOptions, CSharpCompilationOptions> {
        internal MirrorSharpCSharpOptions() : base(
            new CSharpParseOptions(),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary),
            ImmutableList.Create<MetadataReference>(MetadataReference.CreateFromFile(typeof(object).GetTypeInfo().Assembly.Location)),
            ImmutableList.Create<AnalyzerReference>(CreateAnalyzerReference("Microsoft.CodeAnalysis.CSharp.Features"))
        ) {
        }
    }
}
