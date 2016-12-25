using System;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using MirrorSharp.Advanced;
using MirrorSharp.Internal;

namespace MirrorSharp {
    [PublicAPI]
    public sealed class MirrorSharpOptions : IConnectionOptions, IWorkSessionOptions {
        public Func<string, ParseOptions> GetDefaultParseOptionsByLanguageName { get; set; }
        public bool SelfDebugEnabled { get; set; }
        [CanBeNull] public ISlowUpdateExtension SlowUpdate { get; set; }
    }
}

