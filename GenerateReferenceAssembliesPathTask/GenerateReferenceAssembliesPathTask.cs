using System;
using System.IO;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace GenerateReferenceAssembliesPathTask;

// ReSharper disable once UnusedType.Global
public class GenerateReferenceAssembliesPathTask : Task {

    [Required]
    // ReSharper disable once MemberCanBePrivate.Global
    // ReSharper disable once UnusedAutoPropertyAccessor.Global
    public string? TargetFrameworkRootPath { get; set; }

    [Required]
    // ReSharper disable once MemberCanBePrivate.Global
    // ReSharper disable once UnusedAutoPropertyAccessor.Global
    public string? TargetFrameworkVersion { get; set; }

    [Output]
    // ReSharper disable once MemberCanBePrivate.Global
    public string? ClassNameFile { get; set; }

    public override bool Execute() {
        CreateClass();
        return !Log.HasLoggedErrors;
    }

    private void CreateClass() {
        try {
            ClassNameFile = "MirrorSharpOptionsWithXmlDocumentation.generated.cs";
            File.Delete(ClassNameFile);

            // ReSharper disable once ExpressionIsAlwaysNull
            var settingsClass = $@"namespace MirrorSharp.Tests;

public static partial class MirrorSharpOptionsWithXmlDocumentation {{
#if NETFRAMEWORK
    private const string MscorlibReferenceAssemblyPath =
        @""{TargetFrameworkRootPath}.NETFramework\{TargetFrameworkVersion}\mscorlib.dll"";
#endif
}}
";
            File.WriteAllText(ClassNameFile, settingsClass);
        } catch (Exception ex) {
            Log.LogErrorFromException(ex, showStackTrace: true);
        }
    }
}
