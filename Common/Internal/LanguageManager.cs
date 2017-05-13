using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using MirrorSharp.Internal.Abstraction;
using MirrorSharp.Internal.Roslyn;

namespace MirrorSharp.Internal {
    internal class LanguageManager {
        private readonly IDictionary<string, ILanguage> _languages = new Dictionary<string, ILanguage>();

        public LanguageManager(IReadOnlyCollection<string> languageNames) {
            foreach (var name in languageNames) {
                if (_languages.ContainsKey(name))
                    continue;

                _languages.Add(name, CreateLanguage(name));
            }
        }

        private ILanguage CreateLanguage(string name) {
            if (name == LanguageNames.CSharp)
                return new CSharpLanguage();

            if (name == LanguageNames.VisualBasic)
                return new VisualBasicLanguage();

            if (name == "F#") {
                var type = Type.GetType("MirrorSharp.FSharp.FSharpLanguage, MirrorSharp.FSharp", true);
                return (ILanguage)Activator.CreateInstance(type);
            }

            throw new NotSupportedException($"Language '{name}' is not currently supported.");
        }

        public ILanguage GetLanguage(string name) {
            if (!_languages.TryGetValue(name, out ILanguage language))
                throw new Exception($"Language '{name}' was not registered in {nameof(MirrorSharpOptions)}.{nameof(MirrorSharpOptions.LanguageNames)}.");

            return language;
        }
    }
}
