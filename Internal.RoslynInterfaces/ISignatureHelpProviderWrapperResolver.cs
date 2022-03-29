using System.Collections.Generic;

namespace MirrorSharp.Internal.RoslynInterfaces {
    internal interface ISignatureHelpProviderWrapperResolver {
        IEnumerable<ISignatureHelpProviderWrapper> GetAllSlow(string languageName);
    }
}
