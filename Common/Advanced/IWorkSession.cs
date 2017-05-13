using System.Collections.Generic;
using JetBrains.Annotations;

namespace MirrorSharp.Advanced {
    [PublicAPI]
    public interface IWorkSession {
        [NotNull] IRoslynSession Roslyn { get; }
        [CanBeNull] IRoslynSession RoslynOrNull { get; }

        [NotNull] IDictionary<string, object> ExtensionData { get; }
    }
}