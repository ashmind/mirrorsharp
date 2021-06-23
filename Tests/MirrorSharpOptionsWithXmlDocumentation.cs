using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace MirrorSharp.Tests {
    public static partial class MirrorSharpOptionsWithXmlDocumentation {
        #if NETCOREAPP3_0
        private static readonly string MscorlibReferenceAssemblyPath =
            AppDomain.CurrentDomain.BaseDirectory
                + @"\ref-assemblies\System.Runtime.dll";
        #elif NET471
        private static readonly string MscorlibReferenceAssemblyPath =
            Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86)
                + @"\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.7.1\mscorlib.dll";
        #endif

        public static MirrorSharpOptions Instance { get; } = new MirrorSharpOptions().SetupCSharp(c => {
            c.MetadataReferences = ImmutableList<MetadataReference>.Empty;
            c.AddMetadataReferencesFromFiles(MscorlibReferenceAssemblyPath!);
        });
    }
}
