using JetBrains.Annotations;

// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable CollectionNeverUpdated.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace MirrorSharp.Testing.Internal.Results {
    internal class SignaturesItemPart {
        [CanBeNull] public string Text { get; set; }
        public string Kind { get; set; }
        public bool Selected { get; set; }
    }
}