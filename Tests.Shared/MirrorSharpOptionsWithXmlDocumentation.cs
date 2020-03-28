using System.Collections.Immutable;
using System.IO;
using Microsoft.CodeAnalysis;

namespace MirrorSharp.Tests {
    public static partial class MirrorSharpOptionsWithXmlDocumentation {
        public static MirrorSharpOptions Instance { get; } = new MirrorSharpOptions().SetupCSharp(c => {
            c.MetadataReferences = ImmutableList<MetadataReference>.Empty;

            var mscorlibXmlDocumentationPath = Path.ChangeExtension(MscorlibReferenceAssemblyPath, ".xml");
            if (!File.Exists(mscorlibXmlDocumentationPath))
                throw new FileNotFoundException($"Documentation file '{mscorlibXmlDocumentationPath}' was not found.");

            c.AddMetadataReferencesFromFiles(MscorlibReferenceAssemblyPath);
        });
    }
}
