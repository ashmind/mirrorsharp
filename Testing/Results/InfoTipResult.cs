using System.Collections.Generic;
using JetBrains.Annotations;

namespace MirrorSharp.Testing.Results {
    public class InfoTipResult {
        [NotNull] public ResultSpan Span { get; } = new ResultSpan();
        [NotNull] public IList<string> Kinds { get; } = new List<string>();
        [NotNull] public IList<InfoTipSection> Sections { get; } = new List<InfoTipSection>();

        public override string ToString() => string.Join("\r\n", Sections);
    }
}
