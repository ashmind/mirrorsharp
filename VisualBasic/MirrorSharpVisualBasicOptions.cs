using System.Collections.Immutable;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.VisualBasic;
using MirrorSharp.Advanced;

namespace MirrorSharp.VisualBasic {
    /// <summary>MirrorSharp options for Visual Basic .NET</summary>
    public class MirrorSharpVisualBasicOptions : MirrorSharpRoslynOptions<MirrorSharpVisualBasicOptions, VisualBasicParseOptions, VisualBasicCompilationOptions> {
        internal MirrorSharpVisualBasicOptions() : base(
            new VisualBasicParseOptions(),
            new VisualBasicCompilationOptions(OutputKind.DynamicallyLinkedLibrary),
            ImmutableList.Create<MetadataReference>(MetadataReference.CreateFromFile(typeof(object).GetTypeInfo().Assembly.Location)),
            ImmutableList.Create<AnalyzerReference>(CreateAnalyzerReference("Microsoft.CodeAnalysis.VisualBasic.Features"))
        ) {
        }
    }
}
