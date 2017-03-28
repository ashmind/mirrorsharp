using System.Collections.Generic;
using System.Linq;
using AshMind.Extensions;
using JetBrains.Annotations;

// ReSharper disable HeapView.ClosureAllocation
// ReSharper disable HeapView.DelegateAllocation
// ReSharper disable HeapView.ObjectAllocation

namespace MirrorSharp.Testing.Internal.Results {
    internal class SignaturesItem {
        public bool Selected { get; [UsedImplicitly] set; }
        [NotNull, UsedImplicitly] public IList<SignaturesItemPart> Parts { get; } = new List<SignaturesItemPart>();

        public override string ToString() => ToString(true);
        [NotNull]
        public string ToString(bool markSelected) {
            return string.Join("", Parts.GroupAdjacentBy(p => markSelected && p.Selected ? "*" : "").Select(g => g.Key + string.Join("", g.Select(p => p.Text)) + g.Key));
        }
    }
}