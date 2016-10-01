using System.Collections.Generic;
using JetBrains.Annotations;

namespace MirrorSharp.Tests.Internal {
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class SlowUpdateResult {
        public IList<ResultDiagnostic> Diagnostics { get; } = new List<ResultDiagnostic>();

        [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
        public class ResultDiagnostic {
            public string Message { get; set; }
            public string Severity { get; set; }
            public IList<string> Tags { get; } = new List<string>();
            public IList<ResultAction> Actions { get; } = new List<ResultAction>();
        }

        [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
        public class ResultAction {
            public int Id { get; set; }
            public string Title { get; set; }
        }
    }
}