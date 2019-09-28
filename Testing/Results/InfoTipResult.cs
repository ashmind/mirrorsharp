using System.Collections.Generic;

namespace MirrorSharp.Testing.Results {
    public class InfoTipResult {
        public ResultSpan Span { get; } = new ResultSpan();
        public IList<string> Kinds { get; } = new List<string>();
        public IList<InfoTipSection> Sections { get; } = new List<InfoTipSection>();

        public override string ToString() => string.Join("\r\n", Sections);
    }
}
