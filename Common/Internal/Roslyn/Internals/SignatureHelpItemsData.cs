using System.Collections.Generic;
using Microsoft.CodeAnalysis.Text;

namespace MirrorSharp.Internal.Roslyn.Internals {
    internal class SignatureHelpItemsData {
        public SignatureHelpItemsData(
            IEnumerable<SignatureHelpItemData> items,
            TextSpan applicableSpan,
            int argumentIndex,
            int argumentCount,
            int? selectedItemIndex
        ) {
            Items = items;
            ApplicableSpan = applicableSpan;
            ArgumentIndex = argumentIndex;
            ArgumentCount = argumentCount;
            SelectedItemIndex = selectedItemIndex;
        }

        public IEnumerable<SignatureHelpItemData> Items { get; }
        public TextSpan ApplicableSpan { get; }
        public int ArgumentIndex { get; }
        public int ArgumentCount { get; }
        public int? SelectedItemIndex { get; }
    }
}