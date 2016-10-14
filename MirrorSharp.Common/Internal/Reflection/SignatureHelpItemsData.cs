using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.CodeAnalysis.Text;

namespace MirrorSharp.Internal.Reflection {
    internal class SignatureHelpItemsData {
        public SignatureHelpItemsData(IEnumerable<SignatureHelpItemWrapper> itemWrappers, TextSpan applicableSpan) {
            ItemWrappers = itemWrappers;
            ApplicableSpan = applicableSpan;
        }

        public IEnumerable<SignatureHelpItemWrapper> ItemWrappers { get; }
        public TextSpan ApplicableSpan { get; }

        public static Expression FromInternalTypeExpressionSlow(Expression expression) {
            var selectItems = ExpressionHelper.EnumerableSelectSlow(
                expression.Property("Items"),
                RoslynTypes.SignatureHelpItem.AsType(),
                SignatureHelpItemWrapper.FromInternalTypeExpressionSlow,
                typeof(SignatureHelpItemWrapper)
            );

            return Expression.New(
                typeof(SignatureHelpItemsData).GetTypeInfo().GetConstructors()[0],
                selectItems,
                expression.Property("ApplicableSpan")
            );
        }
    }
}