using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using MirrorSharp.Advanced;
using MirrorSharp.Internal;
using MirrorSharp.Internal.Abstraction;
using MirrorSharp.Internal.Roslyn;

namespace MirrorSharp {
    /// <summary>MirrorSharp options object.</summary>
    [PublicAPI]
    public sealed class MirrorSharpOptions : IConnectionOptions, IWorkSessionOptions, ILanguageManagerOptions {
        [NotNull] internal IDictionary<string, Func<ILanguage>> Languages { get; } = new Dictionary<string, Func<ILanguage>>();

        /// <summary>Creates a new instance of <see cref="MirrorSharpOptions" />.</summary>
        public MirrorSharpOptions() {
            Languages.Add(LanguageNames.CSharp, () => new CSharpLanguage(CSharp));
        }

        /// <summary>MirrorSharp options for C#.</summary>
        /// <remarks>These options are ignored if <see cref="DisableCSharp" /> was called.</remarks>
        [NotNull] public MirrorSharpCSharpOptions CSharp { get; } = new MirrorSharpCSharpOptions();

        /// <summary>Defines a <see cref="ISetOptionsFromClientExtension" /> used to support extra options.</summary>
        [CanBeNull] public ISetOptionsFromClientExtension SetOptionsFromClient { get; set; }

        /// <summary>Defines a <see cref="ISlowUpdateExtension" /> used to extend periodic processing.</summary>
        [CanBeNull] public ISlowUpdateExtension SlowUpdate { get; set; }

        /// <summary>Defines a <see cref="IExceptionLogger" /> called for any unhandled exception.</summary>
        public IExceptionLogger ExceptionLogger { get; set; }

        /// <summary>Defines whether the exceptions should include full details (messages, stack traces).</summary>
        public bool IncludeExceptionDetails { get; set; }

        /// <summary>Defines whether the SelfDebug mode is enabled — might reduce performance.</summary>
        public bool SelfDebugEnabled { get; set; }

        /// <summary>Disables C# — the language will not be available to the client.</summary>
        /// <returns>Current <see cref="MirrorSharpOptions" /> object, for convenience.</returns>
        public MirrorSharpOptions DisableCSharp() {
            Languages.Remove(LanguageNames.CSharp);
            return this;
        }

        IDictionary<string, Func<ILanguage>> ILanguageManagerOptions.Languages => Languages;
    }
}

