using System.Threading.Tasks;

namespace MirrorSharp.Internal {
    public interface IWorkSession : IAsyncDisposable {
        void ReplaceText(int start, int length, string newText, int cursorPositionAfter);
        void MoveCursor(int cursorPosition);
        Task<TypeCharResult> TypeCharAsync(char @char);
    }
}