using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
#if QUICKINFO
using System.ComponentModel.Composition;
#endif
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Host.Mef;
using Microsoft.CodeAnalysis.Options;
using TypeInfo = System.Reflection.TypeInfo;

namespace MirrorSharp.Internal.Reflection {
    internal static class RoslynReflection {
        private static readonly BindingFlags DefaultInstanceBindingFlags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance;

        // Roslyn v2
        private static readonly Func<CodeAction, bool> _getIsInlinable =
            RoslynTypes.CodeAction
                .GetProperty("IsInlinable", DefaultInstanceBindingFlags)
                ?.GetMethod.CreateDelegate<Func<CodeAction, bool>>();

        private static readonly Func<CodeAction, ImmutableArray<CodeAction>> _getNestedCodeActions =
            RoslynTypes.CodeAction
                .GetProperty("NestedCodeActions", DefaultInstanceBindingFlags)
                ?.GetMethod.CreateDelegate<Func<CodeAction, ImmutableArray<CodeAction>>>();

        private static readonly Func<AnalyzerOptions, OptionSet, Solution, AnalyzerOptions> _newWorkspaceAnalyzerOptions
            = BuildDelegateForConstructorSlow<Func<AnalyzerOptions, OptionSet, Solution, AnalyzerOptions>>(
                  RoslynTypes.WorkspaceAnalyzerOptions
                      .GetConstructors(DefaultInstanceBindingFlags)
                      .FirstOrDefault()
              );

        private static readonly Func<object, OptionSet> _newWorkspaceOptionSet
            = BuildDelegateForConstructorSlow<Func<object, OptionSet>>(
                  RoslynTypes.WorkspaceOptionSet
                      .GetConstructors(DefaultInstanceBindingFlags)
                      .FirstOrDefault(c => c.GetParameters().Length == 1)
              );

        public static bool IsInlinable(CodeAction action) => _getIsInlinable(action);
        public static ImmutableArray<CodeAction> GetNestedCodeActions(CodeAction action) => _getNestedCodeActions(action);

        #if QUICKINFO
        public static IEnumerable<Type> GetEditorFeaturesTypesWithExportsSafeSlow(Assembly assembly) {
            foreach (var type in GetEditorFeaturesTypesSlow(assembly)) {
                IEnumerable<ExportAttribute> exports;
                try {
                    exports = type.GetCustomAttributes<ExportAttribute>();
                }
                catch (Exception ex) when (ex is FileNotFoundException || ex is TypeLoadException) {
                    // skips exports of Visual Studio types
                    continue;
                }
                if (!exports.Any())
                    continue;
                yield return type;
            }
        }

        private static IEnumerable<Type> GetEditorFeaturesTypesSlow(Assembly assembly) {
            try {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException ex) {
                return ex.Types.Where(t => t != null);
            }
        }
        #endif

        public static IEnumerable<Lazy<ISignatureHelpProviderWrapper, OrderableLanguageMetadataData>> GetSignatureHelpProvidersSlow(MefHostServices hostServices) {
            return GetExportsWithOrderableLanguageMetadataSlow<ISignatureHelpProviderWrapper>(
                hostServices,
                RoslynTypes.ISignatureHelpProvider,
                p => new SignatureHelpProviderWrapper(p)
            );
        }

        #if QUICKINFO
        public static IEnumerable<Lazy<IQuickInfoProviderWrapper, OrderableLanguageMetadataData>> GetQuickInfoProvidersSlow(MefHostServices hostServices) {
            return GetExportsWithOrderableLanguageMetadataSlow<IQuickInfoProviderWrapper>(
                hostServices,
                RoslynTypes.IQuickInfoProvider,
                p => new QuickInfoProviderWrapper(p)
            );
        }
        #endif

        private static IEnumerable<Lazy<TExtensionWrapper, OrderableLanguageMetadataData>> GetExportsWithOrderableLanguageMetadataSlow<TExtensionWrapper>(
            MefHostServices hostServices,
            TypeInfo extensionType,
            Func<object, TExtensionWrapper> createWrapper
        ) {
            var mefHostServicesType = typeof(MefHostServices).GetTypeInfo();
            var getExports = EnsureFound(
                mefHostServicesType, "Microsoft.CodeAnalysis.Host.Mef.IMefHostExportProvider.GetExports",
                (t, n) => t.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic)
                    .FirstOrDefault(m => m.Name == n && m.GetGenericArguments().Length == 2)
            );

            var metadataType = mefHostServicesType.Assembly.GetType("Microsoft.CodeAnalysis.Host.Mef.OrderableLanguageMetadata", true).GetTypeInfo();
            var getExportsOfProvider = getExports.MakeGenericMethod(extensionType.AsType(), metadataType.AsType());
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
                yield return new Lazy<TExtensionWrapper, OrderableLanguageMetadataData>(
                    // ReSharper disable once AccessToModifiedClosure
                    () => createWrapper(valueProperty.GetValue(export)),
                    new OrderableLanguageMetadataData(language)
                );
            }
        }

        [NotNull]
        public static OptionSet NewWorkspaceOptionSet() => _newWorkspaceOptionSet(null);

        [NotNull]
        public static AnalyzerOptions NewWorkspaceAnalyzerOptions(AnalyzerOptions options, OptionSet optionSet, Solution solution) =>
            _newWorkspaceAnalyzerOptions(options, optionSet, solution);

        public static TMemberInfo EnsureFound<TMemberInfo>(TypeInfo type, string name, Func<TypeInfo, string, TMemberInfo> getMember) {
            var member = getMember(type, name);
            if (member == null)
                throw new MissingMemberException($"Member '{name}' was not found on {type}.");
            return member;
        }

        private static TDelegate CreateDelegate<TDelegate>(this MethodInfo method) {
            return (TDelegate)(object)method.CreateDelegate(typeof(TDelegate));
        }

        private static TFunc BuildDelegateForConstructorSlow<TFunc>(ConstructorInfo constructor) {
            var parameterTypes = typeof(TFunc).GetTypeInfo().GetGenericArguments();
            var delegateParameters = parameterTypes
                .Take(parameterTypes.Length - 1)
                .Select(Expression.Parameter)
                .ToList();
            var constructorParameters = constructor.GetParameters();
            var arguments = delegateParameters
                .Zip(constructorParameters, (delegateParameter, constructorParameter) => (delegateParameter: delegateParameter, constructorParameter: constructorParameter))
                .Select(x => {
                    if (x.delegateParameter.Type == x.constructorParameter.ParameterType)
                        return (Expression)x.delegateParameter;
                    return x.delegateParameter.Convert(x.constructorParameter.ParameterType);
                });

            return (TFunc)(object)Expression.Lambda(
                typeof(TFunc),
                Expression.New(constructor, arguments),
                delegateParameters
            ).Compile();
        }
    }
}