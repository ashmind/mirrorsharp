using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CodeActions;
using AshMind.Extensions;

namespace MirrorSharp.Internal {
    public static class RoslynInternals {
        private static readonly Func<CodeAction, bool> _getIsInvokable =
            typeof(CodeAction).GetTypeInfo()
                .GetProperty("IsInvokable", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance)
                .GetMethod.CreateDelegate<Func<CodeAction, bool>>();

        public static bool GetIsInvokable(CodeAction action) => _getIsInvokable(action);
    }
}
