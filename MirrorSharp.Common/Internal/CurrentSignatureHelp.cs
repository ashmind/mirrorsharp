using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MirrorSharp.Internal.Reflection;

namespace MirrorSharp.Internal {
    internal struct CurrentSignatureHelp {
        public CurrentSignatureHelp(ISignatureHelpProviderWrapper provider, SignatureHelpItemsData items) {
            Provider = provider;
            Items = items;
        }

        public ISignatureHelpProviderWrapper Provider { get; }
        public SignatureHelpItemsData Items { get; }
    }
}
