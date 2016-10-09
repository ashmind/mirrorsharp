using System;
using System.Collections.Immutable;
using System.Reflection;
using Microsoft.CodeAnalysis.CodeActions;
using AshMind.Extensions;

namespace MirrorSharp.Internal {
    public static class RoslynInternals {
        private static readonly TypeInfo CodeActionTypeInfo = typeof(CodeAction).GetTypeInfo();

        private static readonly Func<CodeAction, bool> _getIsInvokable =
            CodeActionTypeInfo
                .GetProperty("IsInvokable", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance)
                .GetMethod.CreateDelegate<Func<CodeAction, bool>>();

        private static readonly Func<CodeAction, ImmutableArray<CodeAction>> _getCodeActions =
            CodeActionTypeInfo
                .GetMethod("GetCodeActions", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance)
                .CreateDelegate<Func<CodeAction, ImmutableArray<CodeAction>>>();

        public static bool GetIsInvokable(CodeAction action) => _getIsInvokable(action);
        public static ImmutableArray<CodeAction> GetCodeActions(CodeAction action) => _getCodeActions(action);
    }
}
