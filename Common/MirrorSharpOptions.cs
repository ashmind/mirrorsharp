using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using MirrorSharp.Advanced;
using MirrorSharp.Internal;
using MirrorSharp.Internal.Abstraction;
using MirrorSharp.Internal.Roslyn;

namespace MirrorSharp {
    [PublicAPI]
    public sealed class MirrorSharpOptions : IConnectionOptions, IWorkSessionOptions, ILanguageManagerOptions {
        [NotNull] internal IDictionary<string, Func<ILanguage>> Languages { get; } = new Dictionary<string, Func<ILanguage>>();

        public MirrorSharpOptions() {
            Languages.Add(LanguageNames.CSharp, () => new CSharpLanguage(CSharp));
        }

        [NotNull] public MirrorSharpCSharpOptions CSharp { get; } = new MirrorSharpCSharpOptions();
        [CanBeNull] public ISlowUpdateExtension SlowUpdate { get; set; }
        [CanBeNull] public ISetOptionsFromClientExtension SetOptionsFromClient { get; set; }

        public bool IncludeExceptionDetails { get; set; }
        public bool SelfDebugEnabled { get; set; }

        public MirrorSharpOptions DisableCSharp() {
            Languages.Remove(LanguageNames.CSharp);
            return this;
        }

        IDictionary<string, Func<ILanguage>> ILanguageManagerOptions.Languages => Languages;
    }
}

