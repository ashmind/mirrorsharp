using System.Collections.Generic;
using MirrorSharp.Testing.Results;

// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable CollectionNeverUpdated.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace MirrorSharp.Testing.Internal.Results {
    internal class SignaturesResult {
        public ResultSpan Span { get; } = new ResultSpan();
        public IList<SignaturesItem> Signatures { get; } = new List<SignaturesItem>();
    }
}