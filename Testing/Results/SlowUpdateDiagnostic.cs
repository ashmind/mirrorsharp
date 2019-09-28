using System.Collections.Generic;
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable CollectionNeverUpdated.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace MirrorSharp.Testing.Results {
    public class SlowUpdateDiagnostic {
        public SlowUpdateDiagnostic(
            string id,
            string message,
            string severity,
            ResultSpan span
        ) {
            Id = id;
            Message = message;
            Severity = severity;
            Span = span;
        }

        public string Id { get; }
        public string Message { get; }
        public string Severity { get; }
        public ResultSpan Span { get; }
        public IList<string> Tags { get; } = new List<string>();
        public IList<SlowUpdateDiagnosticAction> Actions { get; } = new List<SlowUpdateDiagnosticAction>();

        public override string ToString() {
            return $"{Severity} {Id}: {Message}";
        }
    }
}
