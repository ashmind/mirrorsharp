using System.Collections.Generic;
using System.Text;

// ReSharper disable HeapView.ClosureAllocation
// ReSharper disable HeapView.DelegateAllocation
// ReSharper disable HeapView.ObjectAllocation
// ReSharper disable HeapView.ObjectAllocation.Possible

namespace MirrorSharp.Testing.Internal.Results {
    internal class SignaturesItem {
        public bool Selected { get; set; }
        public IList<SignaturesItemPart> Parts { get; } = new List<SignaturesItemPart>();

        public override string ToString() => ToString(true);

        public string ToString(bool markSelected) {
            var builder = new StringBuilder();
            var inSelected = false;
            foreach (var part in Parts) {
                if (part.Selected != inSelected) {
                    if (markSelected)
                        builder.Append("*");
                    inSelected = part.Selected;
                }
                builder.Append(part.Text);
            }
            return builder.ToString();
        }
    }
}