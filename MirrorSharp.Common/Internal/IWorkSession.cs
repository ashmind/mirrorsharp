using System.Collections.Immutable;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Completion;
using Microsoft.CodeAnalysis.Text;
using MirrorSharp.Internal.Results;

namespace MirrorSharp.Internal {
    public interface IWorkSession : IAsyncDisposable {
        SourceText SourceText { get; }
        int CursorPosition { get; }

        void ReplaceText(int start, int length, string newText, int cursorPositionAfter);
        void MoveCursor(int cursorPosition);
        Task<TypeCharResult> TypeCharAsync(char @char);
        Task<CompletionChange> GetCompletionChangeAsync(int itemIndex);
        Task<ImmutableArray<Diagnostic>> GetDiagnosticsAsync();
    }
}