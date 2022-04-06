using System;
using System.Collections.Generic;
using System.Threading;
using MirrorSharp.Internal.Abstraction;

namespace MirrorSharp.Internal {
    internal class LanguageManager {
        private readonly IDictionary<string, Lazy<ILanguage>> _languages = new Dictionary<string, Lazy<ILanguage>>();

        // This is run only once per app:
        // ReSharper disable HeapView.ClosureAllocation
        // ReSharper disable HeapView.DelegateAllocation
        // ReSharper disable HeapView.ObjectAllocation.Possible
        public LanguageManager(ILanguageManagerOptions options) {
            var context = new LanguageCreationContext();
            foreach (var language in options.Languages) {
                _languages.Add(language.Key, new Lazy<ILanguage>(() => language.Value(context), LazyThreadSafetyMode.ExecutionAndPublication));
            }
        }
        // ReSharper restore HeapView.ObjectAllocation.Possible
        // ReSharper restore HeapView.DelegateAllocation
        // ReSharper restore HeapView.ClosureAllocation

        public ILanguage GetLanguage(string name) {
            if (!_languages.TryGetValue(name, out var lazy))
                throw new Exception($"Language '{name}' was not enabled in MirrorSharpOptions.");

            return lazy.Value;
        }
    }
}
