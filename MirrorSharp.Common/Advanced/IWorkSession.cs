using System.Dynamic;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;

namespace MirrorSharp.Advanced {
    [PublicAPI]
    public interface IWorkSession {
        [NotNull] Project Project { get; }

        [CanBeNull] T Get<T>();
        void Set<T>([CanBeNull] T value);
    }
}