using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;

namespace MirrorSharp.Advanced {
    [PublicAPI]
    public interface IWorkSession {
        [NotNull] Project Project { get; }
        [NotNull] IDictionary<string, object> ExtensionData { get; }
    }
}