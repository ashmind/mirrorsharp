using System;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;

namespace MirrorSharp.Internal.Abstraction {
    internal interface ILanguageSession : IDisposable {
        [NotNull] string GetText();
        void ReplaceText([CanBeNull] string newText, int start = 0, [CanBeNull] int? length = null);

        [NotNull] Task<ImmutableArray<Diagnostic>> GetDiagnosticsAsync(CancellationToken cancellationToken);
    }
}