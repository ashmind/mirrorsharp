using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;

namespace MirrorSharp.Internal.Reflection {
    internal struct SignatureHelpItemData {
        [UsedImplicitly] // see FromInternalTypeExpressionSlow
        public SignatureHelpItemData(
            Func<CancellationToken, IEnumerable<SymbolDisplayPart>> documentationFactory,
            ImmutableArray<SymbolDisplayPart> prefixParts,
            ImmutableArray<SymbolDisplayPart> separatorParts,
            ImmutableArray<SymbolDisplayPart> suffixParts,
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

        public Func<CancellationToken, IEnumerable<SymbolDisplayPart>> DocumentationFactory { get; }
        public ImmutableArray<SymbolDisplayPart> PrefixDisplayParts { get; }
        public ImmutableArray<SymbolDisplayPart> SeparatorDisplayParts { get; }
        public ImmutableArray<SymbolDisplayPart> SuffixDisplayParts { get; }
        public IEnumerable<SignatureHelpParameterData> Parameters { get; }
        public int ParameterCount { get; }

        public static Expression FromInternalTypeExpressionSlow(Expression expression) {
            return Expression.New(
                typeof(SignatureHelpItemData).GetTypeInfo().GetConstructors()[0],
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