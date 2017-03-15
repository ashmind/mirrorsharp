using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;

namespace MirrorSharp.Internal.Reflection {
    internal class SignatureHelpParameterData {
        [UsedImplicitly] // see FromInternalTypeExpressionSlow
        public SignatureHelpParameterData(IList<TaggedText> displayParts, IList<TaggedText> prefixDisplayParts, IList<TaggedText> suffixDisplayParts) {
            DisplayParts = displayParts;
            PrefixDisplayParts = prefixDisplayParts;
            SuffixDisplayParts = suffixDisplayParts;
        }

        // Roslyn v1
        [UsedImplicitly] // see FromInternalTypeExpressionSlow
        public SignatureHelpParameterData(IList<SymbolDisplayPart> displayParts, IList<SymbolDisplayPart> prefixDisplayParts, IList<SymbolDisplayPart> suffixDisplayParts) {
            DisplayParts = displayParts.ToTaggedText();
            PrefixDisplayParts = prefixDisplayParts.ToTaggedText();
            SuffixDisplayParts = suffixDisplayParts.ToTaggedText();
        }

        public IList<TaggedText> DisplayParts { get; }
        public IList<TaggedText> PrefixDisplayParts { get; }
        public IList<TaggedText> SuffixDisplayParts { get; }

        public static Expression FromInternalTypeExpressionSlow(Expression expression) {
            var displayPartsType = expression.Property("DisplayParts").Type;
            var constructor = typeof(SignatureHelpParameterData).GetTypeInfo()
                .GetConstructors()
                .Single(c => c.GetParameters()[0].ParameterType == displayPartsType);

            return Expression.New(
                constructor,
                expression.Property("DisplayParts"),
                expression.Property("PrefixDisplayParts"),
                expression.Property("SuffixDisplayParts")
            );
        }
    }
}
