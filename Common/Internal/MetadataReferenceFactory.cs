using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace MirrorSharp.Internal {
    internal static class MetadataReferenceFactory {
        public static IEnumerable<MetadataReference> CreateFromFiles(IEnumerable<string> paths) {
            return paths.Select(CreateMetadataReferenceWithXmlDocumentation);
        }

        private static MetadataReference CreateMetadataReferenceWithXmlDocumentation(string path) {
            var xmlPath = Path.Combine(Path.GetDirectoryName(path), Path.GetFileNameWithoutExtension(path) + ".xml");
            var documentation = File.Exists(xmlPath) ? XmlDocumentationProvider.CreateFromFile(xmlPath) : null;

            return MetadataReference.CreateFromFile(path, documentation: documentation);
        }
    }
}
