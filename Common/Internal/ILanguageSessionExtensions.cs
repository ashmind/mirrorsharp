using System;
using MirrorSharp.Advanced.EarlyAccess;

namespace MirrorSharp.Internal {
    internal interface ILanguageSessionExtensions {
        IRoslynGuard? RoslynGuard { get; }
    }
}
