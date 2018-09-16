using System.Collections.Generic;
using JetBrains.Annotations;

namespace MirrorSharp.Testing.Internal.Results {
    internal class InfoTipEntry {
        [NotNull] public string Kind { get; set; }
        [NotNull] public IList<InfoTipEntryPart> Parts { get; } = new List<InfoTipEntryPart>();

        public override string ToString() => string.Join("", Parts);
    }
}
