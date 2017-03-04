using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;

namespace MirrorSharp.Internal.Reflection {
    internal struct SignatureHelpItemData {
        [UsedImplicitly] // see FromInternalTypeExpressionSlow
        public SignatureHelpItemData(
           Func<CancellationToken, IEnumerable<TaggedText>> documentationFactory,
           ImmutableArray<TaggedText> prefixParts,
           ImmutableArray<TaggedText> separatorParts,
           ImmutableArray<TaggedText> suffixParts,
           IEnumerable<SignatureHelpParameterData> parameters,
           int parameterCount
        ) {
            DocumentationFactory = documentationFactory;
            PrefixDisplayParts = prefixParts;
            SeparatorDisplayParts = separatorParts;
            SuffixDisplayParts = suffixParts;
            Parameters = parameters;
            ParameterCount = parameterCount;
        }

        // Roslyn v1
        [UsedImplicitly] // see FromInternalTypeExpressionSlow
        public SignatureHelpItemData(
            Func<CancellationToken, IEnumerable<SymbolDisplayPart>> documentationFactory,
            ImmutableArray<SymbolDisplayPart> prefixParts,
            ImmutableArray<SymbolDisplayPart> separatorParts,
            ImmutableArray<SymbolDisplayPart> suffixParts,
            IEnumerable<SignatureHelpParameterData> parameters,
            int parameterCount
        ) {
            DocumentationFactory = documentationFactory != null
                ? (Func<CancellationToken, IEnumerable<TaggedText>>)(t => documentationFactory(t).ToTaggedText())
                : null;
            PrefixDisplayParts = prefixParts.ToTaggedText();
            SeparatorDisplayParts = separatorParts.ToTaggedText();
            SuffixDisplayParts = suffixParts.ToTaggedText();
            Parameters = parameters;
            ParameterCount = parameterCount;
        }

        public Func<CancellationToken, IEnumerable<TaggedText>> DocumentationFactory { get; }
        public ImmutableArray<TaggedText> PrefixDisplayParts { get; }
        public ImmutableArray<TaggedText> SeparatorDisplayParts { get; }
        public ImmutableArray<TaggedText> SuffixDisplayParts { get; }
        public IEnumerable<SignatureHelpParameterData> Parameters { get; }
        public int ParameterCount { get; }

        public static Expression FromInternalTypeExpressionSlow(Expression expression) {
            var displayPartsType = expression.Property("PrefixDisplayParts").Type;
            var constructor = typeof(SignatureHelpItemData).GetTypeInfo()
                .GetConstructors()
                .Single(c => c.GetParameters()[1].ParameterType == displayPartsType);

            return Expression.New(
                constructor,
                expression.Property("DocumentationFactory"),
                expression.Property("PrefixDisplayParts"),
                expression.Property("SeparatorDisplayParts"),
                expression.Property("SuffixDisplayParts"),
                ExpressionHelper.EnumerableSelectSlow(
                    expression.Property("Parameters"),
                    RoslynTypes.SignatureHelpParameter.AsType(),
                    SignatureHelpParameterData.FromInternalTypeExpressionSlow,
                    typeof(SignatureHelpParameterData)
                ),
                expression.Property("Parameters").Property("Length")
            );
        }
    }
}