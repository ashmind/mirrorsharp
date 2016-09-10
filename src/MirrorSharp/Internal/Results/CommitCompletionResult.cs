using JetBrains.Annotations;

namespace MirrorSharp.Internal.Results {
    public struct CommitCompletionResult {
        public CommitCompletionResult(string newText, int? newCursorPosition) {
            NewText = newText;
            NewCursorPosition = newCursorPosition;
        }

        [NotNull] public string NewText { get; }
        [CanBeNull] public int? NewCursorPosition { get; }
    }
}