using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;

namespace MirrorSharp.Internal.Reflection {
    internal struct SignatureHelpItemWrapper {
        private static readonly Func<object, IEnumerable<SymbolDisplayPart>> _getAllPartsOrdered = CompileGetAllPartsOrdered();
        private readonly object _item;

        [UsedImplicitly] // see FromInternalTypeExpressionSlow
        public SignatureHelpItemWrapper(object item) {
            _item = item;
        }

        public IEnumerable<SymbolDisplayPart> GetAllPartsOrdered() {
            return _getAllPartsOrdered(_item);
        }

        private static Func<object, IEnumerable<SymbolDisplayPart>> CompileGetAllPartsOrdered() {
            var itemParameter = Expression.Parameter(typeof(object));
            var itemTyped = Expression.Convert(itemParameter, RoslynTypes.SignatureHelpItem.AsType());

            var parts = Expression.Variable(typeof(IList<SymbolDisplayPart>));
            var parameters = Expression.Variable(typeof(ImmutableArray<>).MakeGenericType(RoslynTypes.SignatureHelpParameter.AsType()));
            var parameterIndex = Expression.Variable(typeof(int));
            var parameter = Expression.Variable(RoslynTypes.SignatureHelpParameter.AsType());

            var body = Expression.Block(
                new[] { parts, parameters, parameterIndex, parameter },
                parts.Assign(Expression.New(typeof(List<SymbolDisplayPart>))),
                CallAddDisplayPartRange(parts, itemTyped.Property("PrefixDisplayParts")),
                parameters.Assign(itemTyped.Property("Parameters")),
                ExpressionHelper.ForLoop(
                    parameterIndex.Assign(Expression.Constant(0)),
                    parameterIndex.LessThan(parameters.Property("Length")),
                    parameterIndex.AddAssign(Expression.Constant(1)),
                    Expression.Block(
                        parameter.Assign(parameters.Property("Item", parameterIndex)),
                        Expression.IfThen(
                            parameterIndex.GreaterThan(Expression.Constant(0)),
                            CallAddDisplayPartRange(parts, itemTyped.Property("SeparatorDisplayParts"))
                        ),
                        CallAddDisplayPartRange(parts, parameter.Property("PrefixDisplayParts")),
                        CallAddDisplayPartRange(parts, parameter.Property("DisplayParts")),
                        CallAddDisplayPartRange(parts, parameter.Property("SuffixDisplayParts"))
                    )
                ),
                CallAddDisplayPartRange(parts, itemTyped.Property("SuffixDisplayParts")),
                parts
            );

            return Expression.Lambda<Func<object, IEnumerable<SymbolDisplayPart>>>(
                body, itemParameter
            ).Compile();
        }

        private static MethodCallExpression CallAddDisplayPartRange(Expression collection, Expression parts) {
            var generic = ((Action<ICollection<SymbolDisplayPart>, IEnumerable<SymbolDisplayPart>>)AddDisplayPartRange).GetMethodInfo().GetGenericMethodDefinition();
            var method = generic.MakeGenericMethod(parts.Type);
            return Expression.Call(method, collection, parts);
        }

        private static void AddDisplayPartRange<TEnumerable>(ICollection<SymbolDisplayPart> collection, TEnumerable parts)
            where TEnumerable : IEnumerable<SymbolDisplayPart>
        {
            foreach (var part in parts) {
                collection.Add(part);
            }
        }

        public static Expression FromInternalTypeExpressionSlow(Expression expression) {
            return Expression.New(
                typeof(SignatureHelpItemWrapper).GetTypeInfo().GetConstructors()[0],
                expression
            );
        }
    }
}