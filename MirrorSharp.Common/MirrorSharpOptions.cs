using JetBrains.Annotations;
using MirrorSharp.Advanced;
using MirrorSharp.Internal;

namespace MirrorSharp {
    [PublicAPI]
    public sealed class MirrorSharpOptions : IConnectionOptions {
        public bool SelfDebugEnabled { get; set; }
        [CanBeNull] public ISlowUpdateExtension SlowUpdate { get; set; }
    }
}

