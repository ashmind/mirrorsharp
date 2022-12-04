using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace MirrorSharp.Tests {
    public static partial class MirrorSharpOptionsWithXmlDocumentation {
        #if NETCOREAPP
        private static readonly string MscorlibReferenceAssemblyPath =
            AppDomain.CurrentDomain.BaseDirectory
                + @"\ref-assemblies\System.Runtime.dll";
        #endif

        public static MirrorSharpOptions Instance { get; } = new MirrorSharpOptions().SetupCSharp(c => {
            c.MetadataReferences = ImmutableList<MetadataReference>.Empty;
            c.AddMetadataReferencesFromFiles(MscorlibReferenceAssemblyPath!);
        });
    }
}
