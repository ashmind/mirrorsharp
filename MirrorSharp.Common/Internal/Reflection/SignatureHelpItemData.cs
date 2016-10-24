using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;

namespace MirrorSharp.Internal.Reflection {
    internal struct SignatureHelpItemData {
        [UsedImplicitly] // see FromInternalTypeExpressionSlow
        public SignatureHelpItemData(
            ImmutableArray<SymbolDisplayPart> prefixParts,
            ImmutableArray<SymbolDisplayPart> separatorParts,
            ImmutableArray<SymbolDisplayPart> suffixParts,
            IEnumerable<SignatureHelpParameterData> parameters
        ) {
            PrefixDisplayParts = prefixParts;
            SeparatorDisplayParts = separatorParts;
            SuffixDisplayParts = suffixParts;
            Parameters = parameters;
        }

        public ImmutableArray<SymbolDisplayPart> PrefixDisplayParts { get; }
        public ImmutableArray<SymbolDisplayPart> SeparatorDisplayParts { get; }
        public ImmutableArray<SymbolDisplayPart> SuffixDisplayParts { get; }
        public IEnumerable<SignatureHelpParameterData> Parameters { get; }

        public static Expression FromInternalTypeExpressionSlow(Expression expression) {
            return Expression.New(
                typeof(SignatureHelpItemData).GetTypeInfo().GetConstructors()[0],
                expression.Property("PrefixDisplayParts"),
                expression.Property("SeparatorDisplayParts"),
                expression.Property("SuffixDisplayParts"),
                ExpressionHelper.EnumerableSelectSlow(
                    expression.Property("Parameters"),
                    RoslynTypes.SignatureHelpParameter.AsType(),
                    SignatureHelpParameterData.FromInternalTypeExpressionSlow,
                    typeof(SignatureHelpParameterData)
                )
            );
        }
    }
}