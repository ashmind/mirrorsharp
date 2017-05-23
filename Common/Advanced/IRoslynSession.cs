using JetBrains.Annotations;
using Microsoft.CodeAnalysis;

namespace MirrorSharp.Advanced {
    /// <summary>Represents a user session based on Roslyn.</summary>
    [PublicAPI]
    public interface IRoslynSession {
        /// <summary>Roslyn <see cref="Microsoft.CodeAnalysis.Project"/> associated with the current session.</summary>
        [PublicAPI, NotNull] Project Project { get; }
    }
}
