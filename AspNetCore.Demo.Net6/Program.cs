using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using MirrorSharp;
using MirrorSharp.Advanced;
using MirrorSharp.AspNetCore;
using MirrorSharp.AspNetCore.Demo.Extensions;
using MirrorSharp.AspNetCore.Demo.Library;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<ISetOptionsFromClientExtension, SetOptionsFromClientExtension>();

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();
app.UseWebSockets();

app.MapMirrorSharp(
    "/mirrorsharp",
    new MirrorSharpOptions {
        SelfDebugEnabled = true,
        IncludeExceptionDetails = true
    }
    .SetupCSharp(o => {
        o.MetadataReferences = GetAllReferences().ToImmutableList();
    })
    .EnableFSharp()
    .EnableIL()
);

app.Run();

static IEnumerable<MetadataReference> GetAllReferences() {
    yield return ReferenceAssembly("System.Runtime");
    yield return ReferenceAssembly("System.Collections");
    var assembly = typeof(IScriptGlobals).Assembly;
    yield return MetadataReference.CreateFromFile(assembly.Location);
    foreach (var reference in assembly.GetReferencedAssemblies()) {
        yield return ReferenceAssembly(reference.Name!);
    }
}

static MetadataReference ReferenceAssembly(string name) {
    var rootPath = Path.Combine(AppContext.BaseDirectory, "ref-assemblies");
    var assemblyPath = Path.Combine(rootPath, name + ".dll");
    var documentationPath = Path.Combine(rootPath, name + ".xml");

    return MetadataReference.CreateFromFile(
        assemblyPath, documentation: XmlDocumentationProvider.CreateFromFile(documentationPath)
    );
}
