using System.Collections.Generic;
using JetBrains.Annotations;

// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable CollectionNeverUpdated.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace MirrorSharp.Testing.Internal.Results {
    internal class CompletionsItem {
        [CanBeNull] public string DisplayText { get; set; }
        [CanBeNull] public int? Priority { get; set; }
        [NotNull] public IList<string> Kinds { get; } = new List<string>();
    }
}