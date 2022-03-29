using MirrorSharp.Internal.RoslynInterfaces;

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
