namespace MirrorSharp.Testing.Internal.Results {
    internal class SignaturesItemInfoPart {
        #pragma warning disable CS8618 // Non-nullable field is uninitialized.
        public string? Text { get; set; }
        public string Kind { get; set; }
        #pragma warning restore CS8618 // Non-nullable field is uninitialized.

        public override string ToString() => Text ?? "";
    }
}