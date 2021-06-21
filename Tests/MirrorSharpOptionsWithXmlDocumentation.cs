using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace MirrorSharp.Tests {
    public static partial class MirrorSharpOptionsWithXmlDocumentation {
        #if NETCOREAPP3_0
        private static readonly string MscorlibReferenceAssemblyPath =
            Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles)
                + @"\dotnet\packs\Microsoft.NETCore.App.Ref\3.0.0\ref\netcoreapp3.0\System.Runtime.dll";
        #elif NETCOREAPP2_1
        private static readonly string MscorlibReferenceAssemblyPath =
            Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles)
                + @"\dotnet\sdk\NuGetFallbackFolder\microsoft.netcore.app\2.1.0\ref\netcoreapp2.1\System.Runtime.dll";
        #elif NET461
        private static readonly string MscorlibReferenceAssemblyPath =
            Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86)
                + @"\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.6\mscorlib.dll";
        #endif

        public static MirrorSharpOptions Instance { get; } = new MirrorSharpOptions().SetupCSharp(c => {
            c.MetadataReferences = ImmutableList<MetadataReference>.Empty;
            c.AddMetadataReferencesFromFiles(MscorlibReferenceAssemblyPath!);
        });
    }
}
