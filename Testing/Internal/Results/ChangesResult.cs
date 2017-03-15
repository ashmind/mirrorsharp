using System.Collections.Generic;
using JetBrains.Annotations;

// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable CollectionNeverUpdated.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace MirrorSharp.Testing.Internal.Results {
    internal class ChangesResult {
        [CanBeNull] public string Reason { get; set; }
        [NotNull] public IList<ResultChange> Changes { get; } = new List<ResultChange>();
    }
}
