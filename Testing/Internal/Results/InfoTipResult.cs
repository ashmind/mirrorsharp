using System.Collections.Generic;
using JetBrains.Annotations;

namespace MirrorSharp.Testing.Internal.Results {
    internal class InfoTipResult {
        [NotNull] public ResultSpan Span { get; } = new ResultSpan();
        [NotNull] public IList<string> Kinds { get; } = new List<string>();
        [NotNull] public IList<InfoTipEntry> Entries { get; } = new List<InfoTipEntry>();

        public override string ToString() => string.Join("\r\n", Entries);
    }
}
