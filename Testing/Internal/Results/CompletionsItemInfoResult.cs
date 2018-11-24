using System.Collections.Generic;

namespace MirrorSharp.Testing.Internal.Results {
    internal class CompletionsItemInfoResult {
        public int Index { get; set; }
        public IList<CompletionsItemInfoPart> Parts { get; } = new List<CompletionsItemInfoPart>();
    }
}
