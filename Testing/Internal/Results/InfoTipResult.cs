using System.Collections.Generic;
using JetBrains.Annotations;

namespace MirrorSharp.Testing.Internal.Results {
    internal class InfoTipResult {
        [NotNull] public ResultSpan Span { get; } = new ResultSpan();
        [NotNull] public IList<InfoTipItem> Info { get; } = new List<InfoTipItem>();

        public override string ToString() => string.Join("\r\n", Info);
    }
}
