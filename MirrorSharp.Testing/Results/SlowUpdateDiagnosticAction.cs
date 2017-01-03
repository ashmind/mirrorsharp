using JetBrains.Annotations;

// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable CollectionNeverUpdated.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace MirrorSharp.Testing.Results {
    public class SlowUpdateDiagnosticAction {
        public int Id { get; set; }
        [CanBeNull] public string Title { get; set; }
    }
}
