using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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

        public IList<TaggedText> DisplayParts { get; }
        public IList<TaggedText> PrefixDisplayParts { get; }
        public IList<TaggedText> SuffixDisplayParts { get; }

        [SuppressMessage("ReSharper", "HeapView.ClosureAllocation")]
        [SuppressMessage("ReSharper", "HeapView.DelegateAllocation")]
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
