using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis.Completion;

namespace MirrorSharp.Internal.Results {
    public struct TypeCharResult {
        public TypeCharResult([CanBeNull] CompletionList completions) {
            Completions = completions;
        }

        [CanBeNull] public CompletionList Completions { get; }
    }
}
