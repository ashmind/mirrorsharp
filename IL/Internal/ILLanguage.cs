using System;
using System.Collections.Generic;
using System.Text;
using MirrorSharp.Internal;
using MirrorSharp.Internal.Abstraction;

namespace MirrorSharp.IL.Internal {
    // ReSharper disable once InconsistentNaming
    internal class ILLanguage : ILanguage {
        public static string Name = "IL";
        string ILanguage.Name => Name;

        private readonly MirrorSharpILOptions _options;

        public ILLanguage(MirrorSharpILOptions options) {
            _options = options;
        }

        public ILanguageSessionInternal CreateSession(string text, ILanguageSessionExtensions services)
            => new ILSession(text, _options);
    }
}
