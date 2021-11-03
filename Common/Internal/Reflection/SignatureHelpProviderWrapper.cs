using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;

namespace MirrorSharp.Internal.Reflection {
    internal class SignatureHelpProviderWrapper : ISignatureHelpProviderWrapper {
        private static readonly Func<object, char, bool> _isTriggerCharacter = CompileIsXCharacter("IsTriggerCharacter");
        private static readonly Func<object, char, bool> _isRetriggerCharacter = CompileIsXCharacter("IsRetriggerCharacter");
        private static readonly Func<object, Document, int, SignatureHelpTriggerInfoData, SignatureHelpOptionsData, CancellationToken, Task> _getItemsAsyncWithUntypedResult = CompileGetItemsAsyncWithUntypedResult();
        private static readonly Func<Task, SignatureHelpItemsData?> _convertGetItemsAsyncResult = CompileConvertGetItemsAsyncResult();

        private readonly object _provider;

        public SignatureHelpProviderWrapper(object provider) {
            _provider = provider;
        }

        public bool IsTriggerCharacter(char ch) => _isTriggerCharacter(_provider, ch);
        public bool IsRetriggerCharacter(char ch) => _isRetriggerCharacter(_provider, ch);

        public async Task<SignatureHelpItemsData?> GetItemsAsync(Document document, int position, SignatureHelpTriggerInfoData triggerInfo, SignatureHelpOptionsData options, CancellationToken cancellationToken) {
            var task = _getItemsAsyncWithUntypedResult(_provider, document, position, triggerInfo, options, cancellationToken);
            await task.ConfigureAwait(false);
            return _convertGetItemsAsyncResult(task);
        }

        private static Func<object, char, bool> CompileIsXCharacter(string methodName) {
            var instanceParameter = Expression.Parameter(typeof(object));
            var charParameter = Expression.Parameter(typeof(char));
            var instanceTyped = Expression.Convert(instanceParameter, RoslynTypes.ISignatureHelpProvider.AsType());
            var call = Expression.Call(instanceTyped, RoslynTypes.ISignatureHelpProvider.GetMethod(methodName), charParameter);
            return Expression.Lambda<Func<object, char, bool>>(call, instanceParameter, charParameter).Compile();
        }

        private static Func<object, Document, int, SignatureHelpTriggerInfoData, SignatureHelpOptionsData, CancellationToken, Task> CompileGetItemsAsyncWithUntypedResult() {
            var getItemsAsync = RoslynTypes.ISignatureHelpProvider.GetMethod("GetItemsAsync")!;
            var needsOptionsParameter = getItemsAsync.GetParameters().Length == 5;

            var instanceParameter = Expression.Parameter(typeof(object));
            var documentParameter = Expression.Parameter(typeof(Document));
            var positionParameter = Expression.Parameter(typeof(int));
            var triggerInfoParameter = Expression.Parameter(typeof(SignatureHelpTriggerInfoData));
            var optionsParameter = Expression.Parameter(typeof(SignatureHelpOptionsData));
            var cancellationTokenParameter = Expression.Parameter(typeof(CancellationToken));

            var instanceTyped = instanceParameter.Convert(RoslynTypes.ISignatureHelpProvider.AsType());

            var triggerInfo = SignatureHelpTriggerInfoData.ToInternalTypeExpressionSlow(triggerInfoParameter);
            var options = needsOptionsParameter
                ? SignatureHelpOptionsData.ToInternalTypeExpressionSlow(optionsParameter)
                : null;

            var call = needsOptionsParameter ? Expression.Call(
                instanceTyped, getItemsAsync,
                documentParameter, positionParameter, triggerInfo, options, cancellationTokenParameter
            ) : Expression.Call(
                instanceTyped, getItemsAsync,
                documentParameter, positionParameter, triggerInfo, cancellationTokenParameter
            );

            return Expression.Lambda<Func<object, Document, int, SignatureHelpTriggerInfoData, SignatureHelpOptionsData, CancellationToken, Task>>(
                call, instanceParameter, documentParameter, positionParameter, triggerInfoParameter, optionsParameter, cancellationTokenParameter
            ).Compile();
        }

        private static Func<Task, SignatureHelpItemsData?> CompileConvertGetItemsAsyncResult() {
            var taskParameter = Expression.Parameter(typeof(Task));
            var taskTyped = taskParameter.Convert(typeof(Task<>).MakeGenericType(RoslynTypes.SignatureHelpItems.AsType()));
            var taskResult = taskTyped.Property(nameof(Task<object>.Result));
            var result = Expression.Condition(
                taskResult.NotEqual(Expression.Constant(null, RoslynTypes.SignatureHelpItems.AsType())),
                SignatureHelpItemsData.FromInternalTypeExpressionSlow(taskResult),
                Expression.Constant(null, typeof(SignatureHelpItemsData))
            );

            return Expression.Lambda<Func<Task, SignatureHelpItemsData?>>(result, taskParameter).Compile();
        }
    }
}