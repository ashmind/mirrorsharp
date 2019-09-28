using System.Collections.Generic;

// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable CollectionNeverUpdated.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace MirrorSharp.Testing.Internal.Results {
    internal class CompletionsItem {
        public CompletionsItem(string displayText) {
            DisplayText = displayText;
        }

        public string DisplayText { get; }
        public int? Priority { get; set; }
        public IList<string> Kinds { get; } = new List<string>();
    }
}