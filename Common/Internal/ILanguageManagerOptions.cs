using System;
using System.Collections.Generic;
using System.Text;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.VisualBasic;
using MirrorSharp.Internal.Abstraction;

namespace MirrorSharp.Internal {
    internal interface ILanguageManagerOptions {
        [NotNull] MirrorSharpRoslynOptions<CSharpParseOptions, CSharpCompilationOptions> CSharp { get; }
        [NotNull] MirrorSharpRoslynOptions<VisualBasicParseOptions, VisualBasicCompilationOptions> VisualBasic { get; }
        [NotNull] IDictionary<string, Func<ILanguage>> OtherLanguages { get; }
    }
}
