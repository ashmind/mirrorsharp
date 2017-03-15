using JetBrains.Annotations;

// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable CollectionNeverUpdated.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace MirrorSharp.Testing.Internal.Results {
    internal class ResultChange {
        public int Start { get; set; }
        public int Length { get; set; }
        [CanBeNull] public string Text { get; set; }
    }
}