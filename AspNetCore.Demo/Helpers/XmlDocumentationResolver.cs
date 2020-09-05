using System;
using System.IO;
using Microsoft.CodeAnalysis;

namespace SharpLab.Server.Common.Internal {
    public static class XmlDocumentationResolver {
        public static DocumentationProvider GetProvider(string assemblyPath) {
            var dllName = Path.GetFileName(assemblyPath);

            var xmlName = Path.ChangeExtension(dllName, ".xml");
            if (xmlName == "System.Private.CoreLib.xml" || xmlName == "netstandard.xml")
                xmlName = "System.Runtime.xml";

            var xmlPath = Path.Combine(
                Path.GetDirectoryName(new Uri(typeof(XmlDocumentationResolver).Assembly.EscapedCodeBase).LocalPath)!,
                "xmldocs",
                xmlName
            );

            if (!File.Exists(xmlPath))
                throw new FileNotFoundException($"XML documentation for assembly '{dllName}' was not found at {xmlPath}.");

            return XmlDocumentationProvider.CreateFromFile(xmlPath);
        }
    }
}
