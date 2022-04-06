using System;
using System.Collections.Generic;
using MirrorSharp.Internal.Abstraction;

namespace MirrorSharp.Internal {
    internal interface ILanguageManagerOptions {
        IDictionary<string, Func<LanguageCreationContext, ILanguage>> Languages { get; }
    }
}
