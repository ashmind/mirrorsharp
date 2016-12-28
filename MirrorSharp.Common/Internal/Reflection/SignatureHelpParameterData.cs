using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.CodeAnalysis;

namespace MirrorSharp.Internal.Reflection {
    internal class SignatureHelpParameterData {
        public SignatureHelpParameterData(IList<SymbolDisplayPart> displayParts, IList<SymbolDisplayPart> prefixDisplayParts, IList<SymbolDisplayPart> suffixDisplayParts) {
            DisplayParts = displayParts;
            PrefixDisplayParts = prefixDisplayParts;
            SuffixDisplayParts = suffixDisplayParts;
        }

        public IList<SymbolDisplayPart> DisplayParts { get; }
        public IList<SymbolDisplayPart> PrefixDisplayParts { get; }
        public IList<SymbolDisplayPart> SuffixDisplayParts { get; }

        public static Expression FromInternalTypeExpressionSlow(Expression expression) {
            return Expression.New(
                typeof(SignatureHelpParameterData).GetTypeInfo().GetConstructors()[0],
                expression.Property("DisplayParts"),
                expression.Property("PrefixDisplayParts"),
                expression.Property("SuffixDisplayParts")
            );
        }
    }
}
