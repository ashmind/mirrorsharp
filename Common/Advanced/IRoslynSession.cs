using System;
using System.Collections.Generic;
using System.Text;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;

namespace MirrorSharp.Advanced {
    public interface IRoslynSession {
        [NotNull] Project Project { get; }
    }
}
