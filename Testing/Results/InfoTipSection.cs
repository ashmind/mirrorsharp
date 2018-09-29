using System.Collections.Generic;
using JetBrains.Annotations;

namespace MirrorSharp.Testing.Results {
    public class InfoTipSection {
        [NotNull] public string Kind { get; set; }
        [NotNull] public IList<InfoTipSectionPart> Parts { get; } = new List<InfoTipSectionPart>();

        public override string ToString() => string.Join("", Parts);
    }
}
