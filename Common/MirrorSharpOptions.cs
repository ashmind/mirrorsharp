using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using MirrorSharp.Advanced;
using MirrorSharp.Internal;
using MirrorSharp.Internal.Abstraction;
using MirrorSharp.Internal.Roslyn;

namespace MirrorSharp {
    /// <summary>MirrorSharp options object.</summary>
    public sealed class MirrorSharpOptions : IMiddlewareOptions {
        internal IDictionary<string, Func<LanguageCreationContext, ILanguage>> Languages { get; } = new Dictionary<string, Func<LanguageCreationContext, ILanguage>>();

        /// <summary>Creates a new instance of <see cref="MirrorSharpOptions" />.</summary>
        public MirrorSharpOptions() {
            Languages.Add(LanguageNames.CSharp, c => new CSharpLanguage(c, CSharp));
        }

        /// <summary>MirrorSharp options for C#.</summary>
        /// <remarks>These options are ignored if <see cref="DisableCSharp" /> was called.</remarks>
        public MirrorSharpCSharpOptions CSharp { get; } = new MirrorSharpCSharpOptions();

        /// <summary>Defines a <see cref="ISetOptionsFromClientExtension" /> used to support extra options.</summary>
        [Obsolete("ASP.NET Core: register ISetOptionsFromClientExtension in service collection instead. Owin: pass using MirrorSharpServices instead. This property will be removed in the next major version.")]
        public ISetOptionsFromClientExtension? SetOptionsFromClient { get; set; }

        /// <summary>Defines a <see cref="ISlowUpdateExtension" /> used to extend periodic processing.</summary>
        [Obsolete("ASP.NET Core: register ISlowUpdateExtension in service collection instead. Owin: pass using MirrorSharpServices instead. This property will be removed in the next major version.")]
        public ISlowUpdateExtension? SlowUpdate { get; set; }

        /// <summary>Defines a <see cref="IExceptionLogger" /> called for any unhandled exception.</summary>
        [Obsolete("ASP.NET Core: register IExceptionLogger in service collection instead. Owin: pass using MirrorSharpServices instead. This property will be removed in the next major version.")]
        public IExceptionLogger? ExceptionLogger { get; set; }

        /// <summary>Defines whether the exceptions should include full details (messages, stack traces).</summary>
        public bool IncludeExceptionDetails { get; set; }

        /// <summary>Defines whether the SelfDebug mode is enabled — might reduce performance.</summary>
        public bool SelfDebugEnabled { get; set; }

        internal IList<(char commandId, string commandText)> StatusTestCommands { get; } = new List<(char commandId, string commandText)>();

        /// <summary>Disables C# — the language will not be available to the client.</summary>
        /// <returns>Current <see cref="MirrorSharpOptions" /> object, for convenience.</returns>
        public MirrorSharpOptions DisableCSharp() {
            Languages.Remove(LanguageNames.CSharp);
            return this;
        }

        /// <summary>Configures C# support in the <see cref="MirrorSharpOptions" />.</summary>
        /// <param name="setup">Setup delegate used to configure <see cref="MirrorSharpCSharpOptions" /></param>
        /// <returns>Current <see cref="MirrorSharpOptions" /> object, for convenience.</returns>
        public MirrorSharpOptions SetupCSharp(Action<MirrorSharpCSharpOptions> setup) {
            setup(CSharp);
            return this;
        }

        IDictionary<string, Func<LanguageCreationContext, ILanguage>> ILanguageManagerOptions.Languages => Languages;
        IList<(char commandId, string commandText)> IMiddlewareOptions.StatusTestCommands => StatusTestCommands;
    }
}

