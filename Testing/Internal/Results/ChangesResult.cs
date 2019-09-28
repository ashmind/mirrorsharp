using System.Collections.Generic;

// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable CollectionNeverUpdated.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace MirrorSharp.Testing.Internal.Results {
    internal class ChangesResult {
        public string? Reason { get; set; }
        public IList<ResultChange> Changes { get; } = new List<ResultChange>();
    }
}
