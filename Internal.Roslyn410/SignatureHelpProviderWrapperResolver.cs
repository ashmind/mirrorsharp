using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using Microsoft.CodeAnalysis.Host.Mef;
using Microsoft.CodeAnalysis.Shared.Utilities;
using Microsoft.CodeAnalysis.SignatureHelp;
using MirrorSharp.Internal.Roslyn.Internals;

namespace MirrorSharp.Internal.Roslyn410 {
    [Export(typeof(ISignatureHelpProviderWrapperResolver))]
    internal class SignatureHelpProviderWrapperResolver : ISignatureHelpProviderWrapperResolver {
        private readonly IList<Lazy<ISignatureHelpProvider, OrderableLanguageMetadata>> _allProviders;

        [ImportingConstructor]
        public SignatureHelpProviderWrapperResolver(
            [ImportMany] IEnumerable<Lazy<ISignatureHelpProvider, OrderableLanguageMetadata>> allProviders
        ) {
            _allProviders = ExtensionOrderer.Order(allProviders);
        }

        public IEnumerable<ISignatureHelpProviderWrapper> GetAllSlow(string languageName) {
            if (languageName == null)
                throw new ArgumentNullException(nameof(languageName));

            return _allProviders
                .Where(l => l.Metadata.Language == languageName)
                .Select(l => new SignatureHelpProviderWrapper(l.Value));
        }
    }
}
