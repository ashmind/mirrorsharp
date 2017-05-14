using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.VisualBasic;

namespace MirrorSharp.Internal {
    internal interface IWorkSessionOptions {
        bool SelfDebugEnabled { get; }
    }
}