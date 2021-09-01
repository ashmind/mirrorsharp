using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Completion;
using Microsoft.CodeAnalysis.Text;
using MirrorSharp.IL.Advanced;
using MirrorSharp.Internal.Abstraction;

namespace MirrorSharp.IL.Internal {
    // ReSharper disable once InconsistentNaming
    internal class ILSession : ILanguageSessionInternal, IILSession {
        private string _text;
        private MirrorSharpILOptions _options;

        public ILSession(string text, MirrorSharpILOptions options) {
            _text = text;
            _options = options;
        }

        public string GetText() => _text;

        public void ReplaceText(string? newText, int start = 0, int? length = null) {
            if (length > 0)
                _text = _text.Remove(start, length.Value);
            if (newText?.Length > 0)
                _text = _text.Insert(start, newText);
        }

        public Task<ImmutableArray<Diagnostic>> GetDiagnosticsAsync(CancellationToken cancellationToken) {
            // TODO: Implement parsing and returning errors
            return Task.FromResult(ImmutableArray<Diagnostic>.Empty);
        }

        public bool ShouldTriggerCompletion(int cursorPosition, CompletionTrigger trigger)
            => false; // not supported yet

        public Task<CompletionList?> GetCompletionsAsync(int cursorPosition, CompletionTrigger trigger, CancellationToken cancellationToken)
            => Task.FromResult<CompletionList?>(CompletionList.Empty); // not supported yet

        public Task<CompletionDescription?> GetCompletionDescriptionAsync(CompletionItem item, CancellationToken cancellationToken)
            => throw new NotSupportedException(); // not supported yet

        public Task<CompletionChange> GetCompletionChangeAsync(TextSpan completionSpan, CompletionItem item, CancellationToken cancellationToken)
            => throw new NotSupportedException(); // not supported yet

        public void Dispose() { }
    }
}
