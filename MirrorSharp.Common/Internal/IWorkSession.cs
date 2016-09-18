using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Completion;
using Microsoft.CodeAnalysis.Text;
using MirrorSharp.Internal.Results;

namespace MirrorSharp.Internal {
    public interface IWorkSession : IDisposable {
        SourceText SourceText { get; }
        int CursorPosition { get; }

        void ReplaceText(int start, int length, string newText, int cursorPositionAfter);
        void MoveCursor(int cursorPosition);
        Task<TypeCharResult> TypeCharAsync(char @char, CancellationToken cancellationToken);
        Task<CompletionChange> GetCompletionChangeAsync(int itemIndex, CancellationToken cancellationToken);
        Task<SlowUpdateResult> GetSlowUpdateAsync(CancellationToken cancellationToken);
    }
}