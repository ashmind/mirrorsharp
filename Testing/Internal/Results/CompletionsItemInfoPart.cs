namespace MirrorSharp.Testing.Internal.Results {
    internal class CompletionsItemInfoPart {
        public CompletionsItemInfoPart(string kind, string text) {
            Kind = kind;
            Text = text;
        }

        public string Kind { get; }
        public string Text { get; }

        public override string ToString() => Text;
    }
}
