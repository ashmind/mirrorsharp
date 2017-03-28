using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.Host.Mef;
using AshMind.Extensions;
using TypeInfo = System.Reflection.TypeInfo;

namespace MirrorSharp.Internal.Reflection {
    internal static class RoslynReflectionFast {
        // Roslyn v2
        private static readonly Func<CodeAction, bool> _getIsInlinable =
            RoslynTypes.CodeAction
                .GetProperty("IsInlinable", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance)
                ?.GetMethod.CreateDelegate<Func<CodeAction, bool>>();

        private static readonly Func<CodeAction, ImmutableArray<CodeAction>> _getNestedCodeActions =
            RoslynTypes.CodeAction
                .GetProperty("NestedCodeActions", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance)
                ?.GetMethod.CreateDelegate<Func<CodeAction, ImmutableArray<CodeAction>>>();

        // Roslyn v1
        private static readonly Func<CodeAction, bool> _getIsInvokable =
            RoslynTypes.CodeAction
                .GetProperty("IsInvokable", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance)
                ?.GetMethod.CreateDelegate<Func<CodeAction, bool>>();

        private static readonly Func<CodeAction, ImmutableArray<CodeAction>> _getCodeActions =
            RoslynTypes.CodeAction
                .GetMethod("GetCodeActions", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance)
                ?.CreateDelegate<Func<CodeAction, ImmutableArray<CodeAction>>>();

        public static bool IsInlinable(CodeAction action) {
            if (_getIsInlinable == null) // Roslyn v1
                return !_getIsInvokable(action);

            return _getIsInlinable(action);
        }

        public static ImmutableArray<CodeAction> GetNestedCodeActions(CodeAction action) {
            if (_getNestedCodeActions == null) // Roslyn v1
                return _getCodeActions(action);

            return _getNestedCodeActions(action);
        }

        public static IEnumerable<Lazy<ISignatureHelpProviderWrapper, OrderableLanguageMetadataData>> GetSignatureHelpProvidersSlow(MefHostServices hostServices) {
            var mefHostServicesType = typeof(MefHostServices).GetTypeInfo();
            var getExports = EnsureFound(
                mefHostServicesType, "Microsoft.CodeAnalysis.Host.Mef.IMefHostExportProvider.GetExports",
                (t, n) => t.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic)
                    .FirstOrDefault(m => m.Name == n && m.GetGenericArguments().Length == 2)
            );

            var metadataType = mefHostServicesType.Assembly.GetType("Microsoft.CodeAnalysis.Host.Mef.OrderableLanguageMetadata", true).GetTypeInfo();
            var getExportsOfProvider = getExports.MakeGenericMethod(RoslynTypes.ISignatureHelpProvider.AsType(), metadataType.AsType());
            var exports = (IEnumerable)getExportsOfProvider.Invoke(hostServices, null);

            var metadataLanguagePropery = EnsureFound(metadataType, "Language", (t, n) => t.GetProperty(n));

            TypeInfo lazyType = null;
            PropertyInfo metadataProperty = null;
            PropertyInfo valueProperty = null;
            foreach (var export in exports) {
                if (lazyType == null) {
                    lazyType = export.GetType().GetTypeInfo();
                    metadataProperty = EnsureFound(lazyType, "Metadata", (t, n) => t.GetProperty(n));
                    valueProperty = EnsureFound(lazyType, "Value", (t, n) => t.GetProperty(n));
                }
                var metadata = metadataProperty.GetValue(export);
                var language = (string)metadataLanguagePropery.GetValue(metadata);
                yield return new Lazy<ISignatureHelpProviderWrapper, OrderableLanguageMetadataData>(
                    // ReSharper disable once AccessToModifiedClosure
                    () => new SignatureHelpProviderWrapper(valueProperty.GetValue(export)),
                    new OrderableLanguageMetadataData(language)
                );
            }
        }

        private static TMemberInfo EnsureFound<TMemberInfo>(TypeInfo type, string name, Func<TypeInfo, string, TMemberInfo> getMember) {
            var member = getMember(type, name);
            if (member == null)
                throw new MissingMemberException($"Member '{name}' was not found on {type}.");
            return member;
        }
    }
}