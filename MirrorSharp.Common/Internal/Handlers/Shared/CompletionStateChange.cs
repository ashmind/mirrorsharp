namespace MirrorSharp.Internal.Handlers.Shared {
    public enum CompletionStateChange : byte {
        // values simplify parsing
        Cancel = (byte)'X',
        Empty = (byte)'E',
        NonEmptyAfterEmpty = (byte)'B'
    }
}