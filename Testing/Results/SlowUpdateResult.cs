using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Newtonsoft.Json;

// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable CollectionNeverUpdated.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace MirrorSharp.Testing.Results {
    public class SlowUpdateResult<TExtensionResult>
        where TExtensionResult : class
    {
        [NotNull] public IList<SlowUpdateDiagnostic> Diagnostics { get; } = new List<SlowUpdateDiagnostic>();
        [PublicAPI] [JsonProperty("x")] [CanBeNull] public TExtensionResult ExtensionResult { get; set; }

        [PublicAPI]
        public string JoinErrors() {
            return string.Join(Environment.NewLine,
                Diagnostics.Where(d => string.Equals(d.Severity, nameof(DiagnosticSeverity.Error), StringComparison.OrdinalIgnoreCase))
            );
        }
    }
}
