namespace MirrorSharp.Testing.Results {
    internal class ErrorResult {
        public ErrorResult(string message) {
            Message = message;
        }

        public string Message { get; }
    }
}
