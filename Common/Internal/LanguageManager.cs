using System;
using System.Collections.Generic;
using System.Threading;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using MirrorSharp.Internal.Abstraction;
using MirrorSharp.Internal.Roslyn;

namespace MirrorSharp.Internal {
    internal class LanguageManager {
        private readonly IDictionary<string, Lazy<ILanguage>> _languages = new Dictionary<string, Lazy<ILanguage>>();

        // This is run only once per app:
        // ReSharper disable HeapView.ClosureAllocation
        // ReSharper disable HeapView.DelegateAllocation
        // ReSharper disable HeapView.ObjectAllocation.Possible
        public LanguageManager([CanBeNull] ILanguageManagerOptions options) {
            _languages.Add(LanguageNames.CSharp, new Lazy<ILanguage>(
                () => new CSharpLanguage(
                    options?.CSharp.ParseOptions,
                    options?.CSharp.CompilationOptions,
                    options?.CSharp.MetadataReferences
                ),
                LazyThreadSafetyMode.ExecutionAndPublication
            ));
            _languages.Add(LanguageNames.VisualBasic, new Lazy<ILanguage>(
                () => new VisualBasicLanguage(
                    options?.VisualBasic.ParseOptions,
                    options?.VisualBasic.CompilationOptions,
                    options?.VisualBasic.MetadataReferences
                ),
                LazyThreadSafetyMode.ExecutionAndPublication
            ));
            if (options == null)
                return;
            foreach (var other in options.OtherLanguages) {
                _languages.Add(other.Key, new Lazy<ILanguage>(other.Value, LazyThreadSafetyMode.ExecutionAndPublication));
            }
        }
        // ReSharper restore HeapView.ObjectAllocation.Possible
        // ReSharper restore HeapView.DelegateAllocation
        // ReSharper restore HeapView.ClosureAllocation

        public ILanguage GetLanguage(string name) {
            if (!_languages.TryGetValue(name, out Lazy<ILanguage> lazy))
                throw new Exception($"Language '{name}' was not enabled in {nameof(MirrorSharpOptions)}.");

            return lazy.Value;
        }


    }
}
