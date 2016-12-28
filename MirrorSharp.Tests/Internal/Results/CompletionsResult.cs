using System.Collections.Generic;

// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable CollectionNeverUpdated.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace MirrorSharp.Tests.Internal.Results {
    public class CompletionsResult {
        public ResultItem Suggestion { get; set; }
        public IList<ResultItem> Completions { get; } = new List<ResultItem>();

        public class ResultItem {
            public string DisplayText { get; set; }
        }
    }
}