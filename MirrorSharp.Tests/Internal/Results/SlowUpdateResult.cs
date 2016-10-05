using System.Collections.Generic;

// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable CollectionNeverUpdated.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace MirrorSharp.Tests.Internal.Results {
    public class SlowUpdateResult {
        public IList<ResultDiagnostic> Diagnostics { get; } = new List<ResultDiagnostic>();

        public class ResultDiagnostic {
            public string Message { get; set; }
            public string Severity { get; set; }
            public IList<string> Tags { get; } = new List<string>();
            public IList<ResultAction> Actions { get; } = new List<ResultAction>();
        }

        public class ResultAction {
            public int Id { get; set; }
            public string Title { get; set; }
        }
    }
}