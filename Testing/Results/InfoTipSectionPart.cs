namespace MirrorSharp.Testing.Results {
    public class InfoTipSectionPart {
        public InfoTipSectionPart(string kind, string text) {
            Kind = kind;
            Text = text;
        }

        public string Kind { get; }
        public string Text { get; }

        public override string? ToString() => Text;
    }
}