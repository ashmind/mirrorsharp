using System.Collections.Generic;
using JetBrains.Annotations;

namespace MirrorSharp.Advanced {
    [PublicAPI]
    public interface IWorkSession {
        bool IsRoslyn { get; }
        [NotNull] IRoslynSession Roslyn { get; }

        [NotNull] IDictionary<string, object> ExtensionData { get; }
    }
}