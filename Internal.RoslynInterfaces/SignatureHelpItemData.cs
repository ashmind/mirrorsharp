using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using Microsoft.CodeAnalysis;

namespace MirrorSharp.Internal.RoslynInterfaces {
    internal struct SignatureHelpItemData {
        // see FromInternalTypeExpressionSlow
        public SignatureHelpItemData(
           Func<CancellationToken, IEnumerable<TaggedText>> documentationFactory,
           ImmutableArray<TaggedText> prefixDisplayParts,
           ImmutableArray<TaggedText> separatorDisplayParts,
           ImmutableArray<TaggedText> suffixDisplayParts,
           IEnumerable<SignatureHelpParameterData> parameters,
           int parameterCount
        ) {
            DocumentationFactory = documentationFactory;
            PrefixDisplayParts = prefixDisplayParts;
            SeparatorDisplayParts = separatorDisplayParts;
            SuffixDisplayParts = suffixDisplayParts;
            Parameters = parameters;
            ParameterCount = parameterCount;
        }

        public Func<CancellationToken, IEnumerable<TaggedText>> DocumentationFactory { get; }
        public ImmutableArray<TaggedText> PrefixDisplayParts { get; }
        public ImmutableArray<TaggedText> SeparatorDisplayParts { get; }
        public ImmutableArray<TaggedText> SuffixDisplayParts { get; }
        public IEnumerable<SignatureHelpParameterData> Parameters { get; }
        public int ParameterCount { get; }
    }
}