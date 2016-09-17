using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace MirrorSharp.Internal.Results {
    public class SlowUpdateResult {
        public SlowUpdateResult(ImmutableArray<Diagnostic> diagnostics) {
            Diagnostics = diagnostics;
        }

        public ImmutableArray<Diagnostic> Diagnostics { get; }
    }
}
