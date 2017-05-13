using MirrorSharp.Internal.Abstraction;
using Microsoft.FSharp.Compiler.SourceCodeServices;
using Microsoft.FSharp.Core;

namespace MirrorSharp.FSharp {
    internal class FSharpSession : ILanguageSession {
        private string _text;
        private bool _textChanged = false;

        private FSharpChecker _checker;

        public FSharpSession(string text) {
            _checker = FSharpChecker.Create(0, false, false, false);
            _text = text;
        }

        public string GetText() {
            return _text;
        }

        public void ReplaceText(string newText, int start = 0, int? length = null) {
            if (length > 0)
                _text = _text.Remove(start, length.Value);
            if (newText?.Length > 0)
                _text = _text.Insert(start, newText);
            _textChanged = true;
        }

        public void Dispose() {
        }
    }
}