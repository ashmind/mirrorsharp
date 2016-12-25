using System;
using Microsoft.CodeAnalysis;

namespace MirrorSharp.Internal {
    public interface IWorkSessionOptions {
        Func<string, ParseOptions> GetDefaultParseOptionsByLanguageName { get; }
        bool SelfDebugEnabled { get; }
    }
}