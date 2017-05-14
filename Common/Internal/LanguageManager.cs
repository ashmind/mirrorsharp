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

        public LanguageManager([CanBeNull] ILanguageManagerOptions options) {
            _languages.Add(
                LanguageNames.CSharp,
                new Lazy<ILanguage>(() => new CSharpLanguage(options?.CSharp.ParseOptions, options?.CSharp.CompilationOptions), LazyThreadSafetyMode.ExecutionAndPublication)
            );
            _languages.Add(
                LanguageNames.VisualBasic,
                new Lazy<ILanguage>(() => new VisualBasicLanguage(options?.VisualBasic.ParseOptions, options?.VisualBasic.CompilationOptions), LazyThreadSafetyMode.ExecutionAndPublication)
            );
            if (options == null)
                return;
            foreach (var other in options.OtherLanguages) {
                _languages.Add(other.Key, new Lazy<ILanguage>(other.Value, LazyThreadSafetyMode.ExecutionAndPublication));
            }
        }

        public ILanguage GetLanguage(string name) {
            if (!_languages.TryGetValue(name, out Lazy<ILanguage> lazy))
                throw new Exception($"Language '{name}' was not enabled in {nameof(MirrorSharpOptions)}.");

            return lazy.Value;
        }


    }
}
