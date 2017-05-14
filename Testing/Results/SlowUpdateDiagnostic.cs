using System.Collections.Generic;
using JetBrains.Annotations;

// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable CollectionNeverUpdated.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace MirrorSharp.Testing.Results {
    public class SlowUpdateDiagnostic {
        [CanBeNull] public string Id { get; set; }
        [CanBeNull] public string Message { get; set; }
        [CanBeNull] public string Severity { get; set; }
        [CanBeNull] public ResultSpan Span { get; set; }
        [NotNull] public IList<string> Tags { get; } = new List<string>();
        [NotNull] public IList<SlowUpdateDiagnosticAction> Actions { get; } = new List<SlowUpdateDiagnosticAction>();

        public override string ToString() {
            return $"{Severity} {Id}: {Message}";
        }
    }
}
