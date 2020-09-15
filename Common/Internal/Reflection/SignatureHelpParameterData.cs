using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using Microsoft.CodeAnalysis;

namespace MirrorSharp.Internal.Reflection {
    internal class SignatureHelpParameterData {
        // see FromInternalTypeExpressionSlow
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

        public static Expression FromInternalTypeExpressionSlow(Expression expression) {
            return Expression.New(
                typeof(SignatureHelpParameterData).GetTypeInfo().GetConstructors()[0],
                expression.Property(nameof(Name)),
                expression.Property(nameof(DocumentationFactory)),
                expression.Property(nameof(DisplayParts)),
                expression.Property(nameof(PrefixDisplayParts)),
                expression.Property(nameof(SuffixDisplayParts))
            );
        }
    }
}
