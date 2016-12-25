using System;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;

namespace MirrorSharp.Internal {
    public interface IWorkSessionOptions {
        [CanBeNull] Func<string, ParseOptions> GetDefaultParseOptionsByLanguageName { get; }
        [CanBeNull] Func<string, CompilationOptions> GetDefaultCompilationOptionsByLanguageName { get; }
        bool SelfDebugEnabled { get; }
    }
}