using System.Collections.Immutable;
using System.IO;
using Microsoft.CodeAnalysis;

namespace MirrorSharp.Tests {
    public static partial class MirrorSharpOptionsWithXmlDocumentation {
        public static MirrorSharpOptions Instance { get; } = new MirrorSharpOptions().SetupCSharp(c => {
            c.MetadataReferences = ImmutableList<MetadataReference>.Empty;
            c.AddMetadataReferencesFromFiles(MscorlibReferenceAssemblyPath);
        });
    }
}
