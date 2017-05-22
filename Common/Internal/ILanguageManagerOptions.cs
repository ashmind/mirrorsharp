using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using MirrorSharp.Internal.Abstraction;

namespace MirrorSharp.Internal {
    internal interface ILanguageManagerOptions {
        [NotNull] IDictionary<string, Func<ILanguage>> Languages { get; }
    }
}
