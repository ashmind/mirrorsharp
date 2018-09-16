using JetBrains.Annotations;

// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable CollectionNeverUpdated.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace MirrorSharp.Testing.Internal.Results {
    internal class InfoTipEntryPart {
        [NotNull] public string Kind { get; set; }
        [CanBeNull] public string Text { get; set; }

        public override string ToString() => Text;
    }
}