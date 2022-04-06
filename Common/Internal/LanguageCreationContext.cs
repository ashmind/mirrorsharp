using System.Collections.Concurrent;

namespace MirrorSharp.Internal {
    internal class LanguageCreationContext {
        public ConcurrentDictionary<object, object> SharedCache { get; } = new();
    }
}
