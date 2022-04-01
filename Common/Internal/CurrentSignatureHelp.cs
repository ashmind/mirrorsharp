using MirrorSharp.Internal.Roslyn.Internals;

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
