using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;

namespace MirrorSharp.Advanced {
    /// <summary>Represents an active user session.</summary>
    [PublicAPI]
    public interface IWorkSession {
        /// <summary>Returns current session language name (e.g. the value of <see cref="LanguageNames.CSharp"/>).</summary>
        [NotNull] string LanguageName { get; }
        /// <summary>Specifies whether the current session is based on Roslyn.</summary>
        bool IsRoslyn { get; }
        /// <summary>Returns associated Roslyn session if any; throws otherwise.</summary>
        [NotNull] IRoslynSession Roslyn { get; }
        /// <summary>Returns current source code handled by the session.</summary>
        [NotNull] string GetText();
        /// <summary>Arbitrary data associated with the current session.</summary>
        [NotNull] IDictionary<string, object> ExtensionData { get; }
    }
}