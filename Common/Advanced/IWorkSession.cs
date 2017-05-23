using System.Collections.Generic;
using JetBrains.Annotations;

namespace MirrorSharp.Advanced {
    /// <summary>Represents an active user session.</summary>
    [PublicAPI]
    public interface IWorkSession {
        /// <summary>Specifies whether the current session is based on Roslyn.</summary>
        bool IsRoslyn { get; }
        /// <summary>Returns associated Roslyn session if any; throws otherwise.</summary>
        [NotNull] IRoslynSession Roslyn { get; }
        /// <summary>Arbitrary data associated with the user session.</summary>
        [NotNull] IDictionary<string, object> ExtensionData { get; }
    }
}