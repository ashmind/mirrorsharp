using System.Collections.Immutable;

namespace MirrorSharp.Internal {
    internal struct CharArrayString {
        public CharArrayString(ImmutableArray<char> chars) {
            Chars = chars;
        }

        public ImmutableArray<char> Chars { get; }
    }
}
