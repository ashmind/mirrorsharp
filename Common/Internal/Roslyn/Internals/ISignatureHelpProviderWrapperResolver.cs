using System.Collections.Generic;

namespace MirrorSharp.Internal.Roslyn.Internals {
    internal interface ISignatureHelpProviderWrapperResolver {
        IEnumerable<ISignatureHelpProviderWrapper> GetAllSlow(string languageName);
    }
}
