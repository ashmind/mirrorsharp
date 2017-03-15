using System.Collections.Generic;
using JetBrains.Annotations;

// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable CollectionNeverUpdated.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace MirrorSharp.Testing.Internal.Results {
    internal class SignaturesResult {
        [NotNull] public ResultSpan Span { get; } = new ResultSpan();
        [NotNull] public IList<SignaturesItem> Signatures { get; } = new List<SignaturesItem>();
    }
}