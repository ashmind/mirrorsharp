using System.Collections.Generic;

// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable CollectionNeverUpdated.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace MirrorSharp.Tests.Internal.Results {
    public class ChangesResult {
        public IList<ResultChange> Changes { get; } = new List<ResultChange>();

        public class ResultChange {
            public int Start { get; set; }
            public int Length { get; set; }
            public string Text { get; set; }
        }
    }
}
