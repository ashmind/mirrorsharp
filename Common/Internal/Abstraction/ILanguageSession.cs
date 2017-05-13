using System;
using JetBrains.Annotations;

namespace MirrorSharp.Internal.Abstraction {
    internal interface ILanguageSession : IDisposable {
        [NotNull] string GetText();
        void ReplaceText([CanBeNull] string newText, int start = 0, [CanBeNull] int? length = null);
    }
}