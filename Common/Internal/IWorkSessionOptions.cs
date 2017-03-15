using System;
using System.Collections.Immutable;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;

namespace MirrorSharp.Internal {
    internal interface IWorkSessionOptions {
        [CanBeNull] Func<string, ParseOptions> GetDefaultParseOptionsByLanguageName { get; }
        [CanBeNull] Func<string, CompilationOptions> GetDefaultCompilationOptionsByLanguageName { get; }
        [CanBeNull] Func<string, ImmutableList<MetadataReference>> GetDefaultMetadataReferencesByLanguageName { get; }
        bool SelfDebugEnabled { get; }
    }
}