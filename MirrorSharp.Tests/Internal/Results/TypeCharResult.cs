using System.Collections.Generic;

// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable CollectionNeverUpdated.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace MirrorSharp.Tests.Internal.Results {
    public class TypeCharResult {
        public ResultCompletions Completions { get; set; }

        public class ResultCompletions {
            public IList<ResultCompletion> List { get; } = new List<ResultCompletion>();
        }

        public class ResultCompletion {
            public string DisplayText { get; set; }
        }
    }
}