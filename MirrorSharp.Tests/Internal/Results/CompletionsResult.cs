using System.Collections.Generic;

// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable CollectionNeverUpdated.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace MirrorSharp.Tests.Internal.Results {
    public class CompletionsResult {
        public IList<ResultCompletion> Completions { get; } = new List<ResultCompletion>();

        public class ResultCompletion {
            public string DisplayText { get; set; }
        }
    }
}