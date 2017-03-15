using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.CodeAnalysis.Text;

namespace MirrorSharp.Internal.Reflection {
    internal class SignatureHelpItemsData {
        public SignatureHelpItemsData(IEnumerable<SignatureHelpItemData> items, TextSpan applicableSpan, int argumentIndex, int argumentCount, int? selectedItem) {
            Items = items;
            ApplicableSpan = applicableSpan;
            ArgumentIndex = argumentIndex;
            ArgumentCount = argumentCount;
            SelectedItemIndex = selectedItem;
        }

        public IEnumerable<SignatureHelpItemData> Items { get; }
        public TextSpan ApplicableSpan { get; }
        public int ArgumentIndex { get; }
        public int ArgumentCount { get; }
        public int? SelectedItemIndex { get; }

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
                expression.Property("ArgumentIndex"),
                expression.Property("ArgumentCount"),
                expression.Property("SelectedItemIndex")
            );
        }
    }
}