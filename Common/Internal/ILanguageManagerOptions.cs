using System;
using System.Collections.Generic;
using MirrorSharp.Internal.Abstraction;

namespace MirrorSharp.Internal {
    internal interface ILanguageManagerOptions {
        IDictionary<string, Func<ILanguage>> Languages { get; }
    }
}
