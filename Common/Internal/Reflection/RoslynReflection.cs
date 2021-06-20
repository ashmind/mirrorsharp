using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Host.Mef;
using Microsoft.CodeAnalysis.Options;
using TypeInfo = System.Reflection.TypeInfo;

namespace MirrorSharp.Internal.Reflection {
    internal static class RoslynReflection {
        private static readonly BindingFlags DefaultInstanceBindingFlags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance;

        private static readonly Func<CodeAction, bool> _getIsInlinable =
            EnsureFound(RoslynTypes.CodeAction, "IsInlinable", static (t, name) => t.GetProperty(name, DefaultInstanceBindingFlags))
                .GetMethod!.CreateDelegate<Func<CodeAction, bool>>();

        private static readonly Func<CodeAction, ImmutableArray<CodeAction>> _getNestedCodeActions =
            EnsureFound(RoslynTypes.CodeAction, "NestedCodeActions", static (t, name) => t.GetProperty(name, DefaultInstanceBindingFlags))
                .GetMethod!.CreateDelegate<Func<CodeAction, ImmutableArray<CodeAction>>>();

        private static readonly Func<AnalyzerOptions, Solution, AnalyzerOptions> _newWorkspaceAnalyzerOptions
            = BuildDelegateForNewWorkspaceAnalyzerOptionsSlow();

        public static bool IsInlinable(CodeAction action) => _getIsInlinable(action);
        public static ImmutableArray<CodeAction> GetNestedCodeActions(CodeAction action) => _getNestedCodeActions(action);

        public static IEnumerable<Lazy<ISignatureHelpProviderWrapper, OrderableLanguageMetadataData>> GetSignatureHelpProvidersSlow(MefHostServices hostServices) {
            return GetExportsWithOrderableLanguageMetadataSlow<ISignatureHelpProviderWrapper>(
                hostServices,
                RoslynTypes.ISignatureHelpProvider,
                p => new SignatureHelpProviderWrapper(p)
            );
        }

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

            var metadataType = mefHostServicesType.Assembly.GetType("Microsoft.CodeAnalysis.Host.Mef.OrderableLanguageMetadata", true)!.GetTypeInfo();
            var getExportsOfProvider = getExports.MakeGenericMethod(extensionType.AsType(), metadataType.AsType());
            var exports = (IEnumerable)getExportsOfProvider.Invoke(hostServices, null)!;

            var metadataLanguageProperty = EnsureFound(metadataType, "Language", (t, n) => t.GetProperty(n));
            TypeInfo? lazyType = null;
            PropertyInfo? metadataProperty = null;
            PropertyInfo? valueProperty = null;
            foreach (var export in exports) {
                if (lazyType == null) {
                    lazyType = export!.GetType().GetTypeInfo();
                    metadataProperty = EnsureFound(lazyType, "Metadata", static (t, n) => t.GetProperty(n));
                    valueProperty = EnsureFound(lazyType, "Value", static (t, n) => t.GetProperty(n));
                }
                var metadata = metadataProperty!.GetValue(export);
                var language = (string)metadataLanguageProperty.GetValue(metadata)!;
                yield return new Lazy<TExtensionWrapper, OrderableLanguageMetadataData>(
                    // ReSharper disable once AccessToModifiedClosure
                    () => createWrapper(valueProperty!.GetValue(export)!),
                    new OrderableLanguageMetadataData(language)
                );
            }
        }

        public static AnalyzerOptions NewWorkspaceAnalyzerOptions(AnalyzerOptions options, Solution solution) =>
            _newWorkspaceAnalyzerOptions(options, solution);

        public static TMemberInfo EnsureFound<TMemberInfo>(TypeInfo type, string name, Func<TypeInfo, string, TMemberInfo?> getMember) {
            var member = getMember(type, name);
            if (member == null)
                throw new MissingMemberException($"Member '{name}' was not found on {type}.");
            return member;
        }

        private static TDelegate CreateDelegate<TDelegate>(this MethodInfo method) {
            return (TDelegate)(object)method.CreateDelegate(typeof(TDelegate));
        }

        private static Func<AnalyzerOptions, Solution, AnalyzerOptions> BuildDelegateForNewWorkspaceAnalyzerOptionsSlow() {
            var constructor = RoslynTypes.WorkspaceAnalyzerOptions
                .GetConstructors(DefaultInstanceBindingFlags)
                .FirstOrDefault();
            var parameters = constructor.GetParameters();

            // before Roslyn 3.6 
            if (parameters.Length == 3 && parameters[1].ParameterType == typeof(OptionSet)) {
                var newWorkspaceOptionSet = BuildDelegateForConstructorSlow<Func<object?, OptionSet>>(
                    RoslynTypes.WorkspaceOptionSet!
                        .GetConstructors(DefaultInstanceBindingFlags)
                        .FirstOrDefault(c => c.GetParameters().Length == 1)
                );
                var newWorkspaceAnalyzerOptions = BuildDelegateForConstructorSlow<Func<AnalyzerOptions, OptionSet, Solution, AnalyzerOptions>>(constructor);
                return (options, solution) => newWorkspaceAnalyzerOptions(options, newWorkspaceOptionSet(null), solution);
            }

            // after Roslyn 3.6
            return BuildDelegateForConstructorSlow<Func<AnalyzerOptions, Solution, AnalyzerOptions>>(constructor);
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