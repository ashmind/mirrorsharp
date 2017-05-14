using JetBrains.Annotations;
using Microsoft.CodeAnalysis;

namespace MirrorSharp.Advanced {
    [PublicAPI]
    public interface IRoslynSession {
        [PublicAPI, NotNull] Project Project { get; }
    }
}
