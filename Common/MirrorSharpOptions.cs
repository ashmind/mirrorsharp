using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.VisualBasic;
using MirrorSharp.Advanced;
using MirrorSharp.Internal;
using MirrorSharp.Internal.Abstraction;

namespace MirrorSharp {
    [PublicAPI]
    public sealed class MirrorSharpOptions : IConnectionOptions, IWorkSessionOptions, ILanguageManagerOptions {
        [NotNull] public MirrorSharpRoslynOptions<CSharpParseOptions, CSharpCompilationOptions> CSharp { get; } = new MirrorSharpRoslynOptions<CSharpParseOptions, CSharpCompilationOptions>();
        [NotNull] public MirrorSharpRoslynOptions<VisualBasicParseOptions, VisualBasicCompilationOptions> VisualBasic { get; } = new MirrorSharpRoslynOptions<VisualBasicParseOptions, VisualBasicCompilationOptions>();
        [NotNull] internal IDictionary<string, Func<ILanguage>> OtherLanguages { get; } = new Dictionary<string, Func<ILanguage>>();

        [CanBeNull] public ISlowUpdateExtension SlowUpdate { get; set; }
        [CanBeNull] public ISetOptionsFromClientExtension SetOptionsFromClient { get; set; }

        public bool IncludeExceptionDetails { get; set; }
        public bool SelfDebugEnabled { get; set; }
        
        IDictionary<string, Func<ILanguage>> ILanguageManagerOptions.OtherLanguages => OtherLanguages;
    }
}

