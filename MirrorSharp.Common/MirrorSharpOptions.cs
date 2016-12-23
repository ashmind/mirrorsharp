using JetBrains.Annotations;
using MirrorSharp.Advanced;
using MirrorSharp.Internal;

namespace MirrorSharp {
    public sealed class MirrorSharpOptions : IConnectionOptions {
        public bool SendDebugCompareMessages { get; set; }
        [CanBeNull] public ISlowUpdateExtension SlowUpdate { get; set; }
    }
}

