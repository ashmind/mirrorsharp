using System;
using System.Collections.Immutable;
using System.IO;
using Microsoft.CodeAnalysis;

namespace MirrorSharp.Tests {
    public static partial class MirrorSharpOptionsWithXmlDocumentation {

        private static readonly string MscorlibReferenceAssemblyPath =
#if NETCOREAPP
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory!, "ref-assemblies", "System.Runtime.dll");
#else
            File
                .ReadAllText(Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory!, "PkgMicrosoft_NETFramework_ReferenceAssemblies.txt"))
                .TrimEnd('\r', '\n');
#endif

        public static MirrorSharpOptions Instance { get; } = new MirrorSharpOptions().SetupCSharp(c => {
            c.MetadataReferences = ImmutableList<MetadataReference>.Empty;
            c.AddMetadataReferencesFromFiles(MscorlibReferenceAssemblyPath!);
        });
    }
}
