using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace MirrorSharp.Internal.Reflection {
    internal static class ExpressionHelper {
        public static Expression EnumerableSelectSlow(Expression collection, Type itemType, Func<Expression, Expression> getSelector, Type resultType) {
            var itemParameter = Expression.Parameter(itemType);
            return Expression.Call(
                typeof(Enumerable), nameof(Enumerable.Select),
                new[] { itemType, resultType },
                Expression.Convert(collection, typeof(IEnumerable<>).MakeGenericType(itemType)),
                Expression.Lambda(getSelector(itemParameter), itemParameter)
            );
        }

        public static UnaryExpression Convert(this Expression expression, Type type) {
            return Expression.Convert(expression, type);
        }

        public static MemberExpression Property(this Expression expression, string propertyName) {
            return Expression.Property(expression, propertyName);
        }

        public static IndexExpression Property(this Expression expression, string propertyName, params Expression[] arguments) {
            return Expression.Property(expression, propertyName, arguments);
        }

        public static MemberExpression Field(this Expression expression, string fieldName) {
            return Expression.Field(expression, fieldName);
        }

        public static MethodCallExpression Call(this Expression expression, string methodName) {
            return Expression.Call(expression, methodName, null);
        }

        public static BinaryExpression Assign(this Expression left, Expression right) {
            return Expression.Assign(left, right);
        }

        public static BinaryExpression AddAssign(this Expression left, Expression right) {
            return Expression.AddAssign(left, right);
        }

        public static BinaryExpression NotEqual(this Expression left, Expression right) {
            return Expression.NotEqual(left, right);
        }

        public static BinaryExpression LessThan(this Expression left, Expression right) {
            return Expression.LessThan(left, right);
        }

        public static BinaryExpression GreaterThan(this Expression left, Expression right) {
            return Expression.GreaterThan(left, right);
        }

        public static Expression ForLoop(Expression start, Expression condition, Expression increment, Expression body) {
            var breakLabel = Expression.Label();
            return Expression.Block(
                start,
                Expression.Loop(Expression.Block(
                    Expression.IfThen(Expression.Not(condition), Expression.Break(breakLabel)),
                    body,
                    increment
                ), breakLabel)
            );
        }
    }
}
