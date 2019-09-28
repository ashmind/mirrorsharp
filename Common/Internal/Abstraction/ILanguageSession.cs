using System;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Completion;
using Microsoft.CodeAnalysis.Text;

namespace MirrorSharp.Internal.Abstraction {
    internal interface ILanguageSessionInternal : IDisposable {
        string GetText();
        void ReplaceText(string? newText, int start = 0, int? length = null);

        Task<ImmutableArray<Diagnostic>> GetDiagnosticsAsync(CancellationToken cancellationToken);

        bool ShouldTriggerCompletion(int cursorPosition, CompletionTrigger trigger);
        Task<CompletionList?> GetCompletionsAsync(int cursorPosition, CompletionTrigger trigger, CancellationToken cancellationToken);
        Task<CompletionDescription?> GetCompletionDescriptionAsync(CompletionItem item, CancellationToken cancellationToken);
        Task<CompletionChange> GetCompletionChangeAsync(TextSpan completionSpan, CompletionItem item, CancellationToken cancellationToken);
    }
}