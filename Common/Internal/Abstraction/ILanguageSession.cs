using System;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Completion;
using Microsoft.CodeAnalysis.Text;

namespace MirrorSharp.Internal.Abstraction {
    internal interface ILanguageSessionInternal : IDisposable {
        [NotNull] string GetText();
        void ReplaceText([CanBeNull] string newText, int start = 0, [CanBeNull] int? length = null);

        [NotNull] Task<ImmutableArray<Diagnostic>> GetDiagnosticsAsync(CancellationToken cancellationToken);

        bool ShouldTriggerCompletion(int cursorPosition, CompletionTrigger trigger);
        [NotNull, ItemCanBeNull] Task<CompletionList> GetCompletionsAsync(int cursorPosition, CompletionTrigger trigger, CancellationToken cancellationToken);
        [NotNull, ItemCanBeNull] Task<CompletionDescription> GetCompletionDescriptionAsync(CompletionItem item, CancellationToken cancellationToken);
        [NotNull, ItemNotNull] Task<CompletionChange> GetCompletionChangeAsync(TextSpan completionSpan, [NotNull] CompletionItem item, CancellationToken cancellationToken);
    }
}