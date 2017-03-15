using System.Collections.Generic;
using System.Linq;
using AshMind.Extensions;
using JetBrains.Annotations;

namespace MirrorSharp.Testing.Internal.Results {
    internal class SignaturesItem {
        public bool Selected { get; set; }
        [NotNull] public IList<SignaturesItemPart> Parts { get; } = new List<SignaturesItemPart>();

        public override string ToString() => ToString(true);
        [NotNull]
        public string ToString(bool markSelected) {
            return string.Join("", Parts.GroupAdjacentBy(p => markSelected && p.Selected ? "*" : "").Select(g => g.Key + string.Join("", g.Select(p => p.Text)) + g.Key));
        }
    }
}