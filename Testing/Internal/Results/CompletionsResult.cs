using System.Collections.Generic;
using JetBrains.Annotations;

// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable CollectionNeverUpdated.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace MirrorSharp.Testing.Internal.Results {
    internal class CompletionsResult {
        [CanBeNull] public CompletionsItem Suggestion { get; set; }
        [NotNull] public IList<CompletionsItem> Completions { get; } = new List<CompletionsItem>();
    }
}