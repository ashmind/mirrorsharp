using JetBrains.Annotations;

namespace MirrorSharp.Testing.Results {
    public class InfoTipSectionPart {
        [NotNull] public string Kind { get; set; }
        [CanBeNull] public string Text { get; set; }

        public override string ToString() => Text;
    }
}