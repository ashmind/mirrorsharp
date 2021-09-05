using System;
using System.Collections.Immutable;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Completion;
using Microsoft.CodeAnalysis.Text;
using MirrorSharp.IL.Advanced;
using MirrorSharp.Internal.Abstraction;
using Mobius.ILasm.Core;

namespace MirrorSharp.IL.Internal {
    // ReSharper disable once InconsistentNaming
    internal class ILSession : ILanguageSessionInternal, IILSession, IILSessionInternal {
        private readonly StringBuilder _textBuilder;
        private string? _text;

        public ILSession(string text) {
            _textBuilder = new StringBuilder(text);
            _text = text;
        }

        public Driver.Target Target { get; set; }

        public int TextLength => _textBuilder.Length;

        public string GetText() {
            _text ??= _textBuilder.ToString();
            return _text;
        }

        public StringBuilder GetTextBuilderForReadsOnly() {
            return _textBuilder;
        }

        public void ReplaceText(string? newText, int start = 0, int? length = null) {
            if (length > 0)
                _textBuilder.Remove(start, length.Value);
            if (newText?.Length > 0)
                _textBuilder.Insert(start, newText);
            _text = null;
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
