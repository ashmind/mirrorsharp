using System.Linq.Expressions;
using System.Reflection;
using Microsoft.CodeAnalysis.Text;

namespace MirrorSharp.Internal.Reflection {
    internal class QuickInfoItemData {
        public TextSpan TextSpan { get; }
        public DeferredQuickInfoContentData Content { get; }

        public QuickInfoItemData(TextSpan textSpan) {
            TextSpan = textSpan;
        }

        internal static Expression FromInternalTypeExpressionSlow(Expression expression) {
            return Expression.New(
                typeof(QuickInfoItemData).GetTypeInfo().GetConstructors()[0],
                expression.Property(nameof(TextSpan))
            );
        }
    }
}
