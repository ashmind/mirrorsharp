namespace MirrorSharp.Testing.Internal.Results {
    internal class CompletionsItemInfoPart {
        public string Kind { get; set; }
        public string Text { get; set; }

        public override string ToString() => Text;
    }
}
