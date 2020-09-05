using System.Collections.Generic;

namespace MirrorSharp.Testing.Internal.Results {
    internal class SignaturesItemInfo {
        public IList<SignaturesItemInfoPart> Parts { get; } = new List<SignaturesItemInfoPart>();
        public SignaturesItemInfoParameter? Parameter { get; set; }
    }
}