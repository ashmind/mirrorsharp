using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.CodeAnalysis.Text;

namespace MirrorSharp.Internal.Reflection {
    internal class SignatureHelpItemsData {
        public SignatureHelpItemsData(IEnumerable<SignatureHelpItemData> items, TextSpan applicableSpan, int argumentIndex) {
            Items = items;
            ApplicableSpan = applicableSpan;
            ArgumentIndex = argumentIndex;
        }

        public IEnumerable<SignatureHelpItemData> Items { get; }
        public TextSpan ApplicableSpan { get; }
        public int ArgumentIndex { get; }

        public static Expression FromInternalTypeExpressionSlow(Expression expression) {
            var selectItems = ExpressionHelper.EnumerableSelectSlow(
                expression.Property("Items"),
                RoslynTypes.SignatureHelpItem.AsType(),
                SignatureHelpItemData.FromInternalTypeExpressionSlow,
                typeof(SignatureHelpItemData)
            );

            return Expression.New(
                typeof(SignatureHelpItemsData).GetTypeInfo().GetConstructors()[0],
                selectItems,
                expression.Property("ApplicableSpan"),
                expression.Property("ArgumentIndex")
            );
        }
    }
}