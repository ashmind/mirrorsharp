#if QUICKINFO
using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;

namespace MirrorSharp.Internal.Reflection {
    internal class QuickInfoProviderWrapper : IQuickInfoProviderWrapper {
        private static readonly Func<object, Document, int, CancellationToken, Task> _getItemAsyncWithUntypedResult = CompileGetItemAsyncWithUntypedResult();
        private static readonly Func<Task, QuickInfoItemData> _convertGetItemAsyncResult = CompileConvertGetItemAsyncResultSlow();

        private readonly object _provider;

        public QuickInfoProviderWrapper(object provider) {
            _provider = provider;
        }

        public async Task<QuickInfoItemData> GetItemAsync(Document document, int position, CancellationToken cancellationToken) {
            var task = _getItemAsyncWithUntypedResult(_provider, document, position, cancellationToken);
            await task.ConfigureAwait(false);
            return _convertGetItemAsyncResult(task);
        }

        private static Func<object, Document, int, CancellationToken, Task> CompileGetItemAsyncWithUntypedResult() {
            var instanceParameter = Expression.Parameter(typeof(object));
            var documentParameter = Expression.Parameter(typeof(Document));
            var cursorPositionParameter = Expression.Parameter(typeof(int));
            var cancellationTokenParameter = Expression.Parameter(typeof(CancellationToken));
            var instanceTyped = instanceParameter.Convert(RoslynTypes.IQuickInfoProvider.AsType());

            var call = Expression.Call(
                instanceTyped, RoslynTypes.ISignatureHelpProvider.GetMethod("GetItemAsync"),
                documentParameter, cursorPositionParameter, cancellationTokenParameter
            );

            return Expression.Lambda<Func<object, Document, int, CancellationToken, Task>>(
                call, instanceParameter, documentParameter, cursorPositionParameter, cancellationTokenParameter
            ).Compile();
        }

        private static Func<Task, QuickInfoItemData> CompileConvertGetItemAsyncResultSlow() {
            var taskParameter = Expression.Parameter(typeof(Task));
            var taskTyped = taskParameter.Convert(typeof(Task<>).MakeGenericType(RoslynTypes.QuickInfoItem.AsType()));
            var taskResult = taskTyped.Property(nameof(Task<object>.Result));
            var result = Expression.Condition(
                taskResult.NotEqual(Expression.Constant(null, RoslynTypes.QuickInfoItem.AsType())),
                QuickInfoItemData.FromInternalTypeExpressionSlow(taskResult),
                Expression.Constant(null, typeof(QuickInfoItemData))
            );

            return Expression.Lambda<Func<Task, QuickInfoItemData>>(result, taskParameter).Compile();
        }
    }
}
#endif