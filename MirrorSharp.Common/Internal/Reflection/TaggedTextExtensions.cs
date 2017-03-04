using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using AshMind.Extensions;
using Microsoft.CodeAnalysis;

namespace MirrorSharp.Internal.Reflection {
    internal static class TaggedTextExtensions {
        private static readonly Lazy<Func<SymbolDisplayPartKind, string>> _getTag = new Lazy<Func<SymbolDisplayPartKind, string>>(
            () => RoslynTypes.SymbolDisplayPartKindTags
                .GetMethod("GetTag", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static)
                .CreateDelegate<Func<SymbolDisplayPartKind, string>>()
        );

        public static ImmutableArray<TaggedText> ToTaggedText(this IEnumerable<SymbolDisplayPart> displayParts) {
            if (displayParts == null)
                return ImmutableArray<TaggedText>.Empty;

            return displayParts.Select(d => new TaggedText(_getTag.Value(d.Kind), d.ToString())).ToImmutableArray();
        }
    }
}
