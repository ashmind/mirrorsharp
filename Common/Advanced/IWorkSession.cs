using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace MirrorSharp.Advanced {
    /// <summary>Represents an active user session.</summary>
    public interface IWorkSession {
        /// <summary>Returns current session language name (e.g. the value of <see cref="LanguageNames.CSharp"/>).</summary>
        string LanguageName { get; }
        /// <summary>Specifies whether the current session is based on Roslyn.</summary>
        bool IsRoslyn { get; }
        /// <summary>Returns associated Roslyn session if any; throws otherwise.</summary>
        IRoslynSession Roslyn { get; }
        /// <summary>Returns current source code handled by the session.</summary>
        string GetText();
        /// <summary>Arbitrary data associated with the current session.</summary>
        IDictionary<string, object> ExtensionData { get; }
    }
}