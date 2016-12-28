using System.Collections;
using System.Collections.Generic;

namespace MirrorSharp.Internal {
    internal struct CharListString : IEnumerable<char> {
        private readonly IReadOnlyList<char> _chars;

        public CharListString(IReadOnlyList<char> chars) {
            _chars = chars;
        }

        public IEnumerator<char> GetEnumerator() => _chars.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => _chars.GetEnumerator();
    }
}
