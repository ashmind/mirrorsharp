using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.CodeAnalysis;
using Newtonsoft.Json;

// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable CollectionNeverUpdated.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace MirrorSharp.Testing.Results {
    public class SlowUpdateResult<TExtensionResult> {
        public IList<SlowUpdateDiagnostic> Diagnostics { get; } = new List<SlowUpdateDiagnostic>();

        #pragma warning disable CS8618 // Non-nullable field is uninitialized.
        // https://github.com/dotnet/roslyn/issues/37511
        [MaybeNull] [JsonProperty("x")]  public TExtensionResult ExtensionResult { get; set; }
        #pragma warning restore CS8618 // Non-nullable field is uninitialized.

        public string JoinErrors() {
            return string.Join(Environment.NewLine,
                Diagnostics.Where(d => string.Equals(d.Severity, nameof(DiagnosticSeverity.Error), StringComparison.OrdinalIgnoreCase))
            );
        }
    }
}
