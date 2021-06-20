using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Microsoft.CodeAnalysis;

namespace MirrorSharp.Internal {
    internal static class MetadataReferenceFactory {
        public static IEnumerable<MetadataReference> CreateFromFilesSlow(IEnumerable<string> paths) {
            return paths.Select(CreateMetadataReferenceWithXmlDocumentationSlow);
        }

        private static MetadataReference CreateMetadataReferenceWithXmlDocumentationSlow(string path) {
            var xmlPath = Path.Combine(Path.GetDirectoryName(path)!, Path.GetFileNameWithoutExtension(path) + ".xml");
            return MetadataReference.CreateFromFile(path, documentation: FindXmlDocumentationSlow(xmlPath));
        }

        private static XmlDocumentationProvider? FindXmlDocumentationSlow(string xmlPath, char[]? charBuffer = null) {
            if (!File.Exists(xmlPath))
                return null;

            // Might not be required once https://github.com/dotnet/roslyn/issues/23685 is done
            const int RedirectCheckBlockLength = 1024;

            string block;
            using (var reader = new StreamReader(xmlPath)) {
                charBuffer ??= new char[RedirectCheckBlockLength];
                var count = reader.ReadBlock(charBuffer, 0, RedirectCheckBlockLength);
                block = new string(charBuffer, 0, count);
            }

            if (block.Length < RedirectCheckBlockLength) {
                var xml = XDocument.Parse(block);
                var redirect = xml.Root.Attribute("redirect")?.Value;
                if (redirect != null)
                    return FindXmlDocumentationSlow(ExpandRedirectVariablesSlow(redirect), charBuffer);
            }

            return XmlDocumentationProvider.CreateFromFile(xmlPath);
        }

        private static string ExpandRedirectVariablesSlow(string redirect) {
            // https://github.com/dotnet/roslyn/issues/13529#issuecomment-245097691
            // only supporting %PROGRAMFILESDIR% for now
            var programFilesDir = Environment
                .GetFolderPath(Environment.SpecialFolder.ProgramFiles)
                .TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
            return redirect.Replace("%PROGRAMFILESDIR%", programFilesDir);
        }
    }
}
