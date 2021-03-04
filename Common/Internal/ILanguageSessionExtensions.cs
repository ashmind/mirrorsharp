using System;
using MirrorSharp.Advanced.EarlyAccess;

namespace MirrorSharp.Internal {
    internal interface ILanguageSessionExtensions {
        IRoslynSourceTextGuard? RoslynSourceTextGuard { get; }
        IRoslynCompilationGuard? RoslynCompilationGuard { get; }
    }
}
