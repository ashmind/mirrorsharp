using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.CodeAnalysis;

namespace MirrorSharp.Internal.RoslynInterfaces {
    internal class SignatureHelpParameterData {
        public SignatureHelpParameterData(
            string name,
            Func<CancellationToken, IEnumerable<TaggedText>> documentationFactory,
            IList<TaggedText> displayParts,
            IList<TaggedText> prefixDisplayParts,
            IList<TaggedText> suffixDisplayParts
        ) {
            Name = name;
            DocumentationFactory = documentationFactory;
            DisplayParts = displayParts;
            PrefixDisplayParts = prefixDisplayParts;
            SuffixDisplayParts = suffixDisplayParts;
        }

        public string Name { get; }
        public Func<CancellationToken, IEnumerable<TaggedText>> DocumentationFactory { get; }
        public IList<TaggedText> DisplayParts { get; }
        public IList<TaggedText> PrefixDisplayParts { get; }
        public IList<TaggedText> SuffixDisplayParts { get; }
    }
}
