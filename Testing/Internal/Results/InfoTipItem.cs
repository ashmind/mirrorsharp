using System;
using System.Collections.Generic;
using System.Text;
using JetBrains.Annotations;

namespace MirrorSharp.Testing.Internal.Results {
    internal class InfoTipItem {
        [NotNull] public string Kind { get; set; }
        [NotNull] public IList<InfoTipItemPart> Parts { get; } = new List<InfoTipItemPart>();

        public override string ToString() => $"<{Kind}> {string.Join("", Parts)}";
    }
}
