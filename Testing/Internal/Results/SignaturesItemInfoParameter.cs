using System.Collections.Generic;

namespace MirrorSharp.Testing.Internal.Results {
    internal class SignaturesItemInfoParameter {
        #pragma warning disable CS8618 // Non-nullable field is uninitialized.
        public string Name { get; set; }
        
        #pragma warning restore CS8618 // Non-nullable field is uninitialized.
        public IList<SignaturesItemInfoPart> Parts { get; } = new List<SignaturesItemInfoPart>();
    }
}