using System.Collections.Generic;

namespace MirrorSharp.Testing.Results {
    public class InfoTipSection {
        public InfoTipSection(string kind) {
            Kind = kind;
        }

        public string Kind { get; }
        public IList<InfoTipSectionPart> Parts { get; } = new List<InfoTipSectionPart>();

        public override string ToString() => string.Join("", Parts);
    }
}
