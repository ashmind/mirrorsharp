using System.Collections.Generic;

// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable CollectionNeverUpdated.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace MirrorSharp.Testing.Internal.Results {
    internal class CompletionsResult {
        public CompletionsItem? Suggestion { get; set; }
        public IList<CompletionsItem> Completions { get; } = new List<CompletionsItem>();
    }
}