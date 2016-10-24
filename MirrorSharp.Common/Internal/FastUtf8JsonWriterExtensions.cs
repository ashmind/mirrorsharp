using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace MirrorSharp.Internal {
    public static class FastUtf8JsonWriterExtensions {
        public static void WriteSpan(this FastUtf8JsonWriter writer, TextSpan span) {
            writer.WriteStartObject();
            writer.WriteProperty("start", span.Start);
            writer.WriteProperty("length", span.Length);
            writer.WriteEndObject();
        }

        public static void WriteChange(this FastUtf8JsonWriter writer, TextChange change) {
            writer.WriteStartObject();
            writer.WriteProperty("start", change.Span.Start);
            writer.WriteProperty("length", change.Span.Length);
            writer.WriteProperty("text", change.NewText);
            writer.WriteEndObject();
        }

        public static void WriteSymbolDisplayParts<TCollection>(this FastUtf8JsonWriter writer, TCollection parts, bool selected = false)
            where TCollection : IEnumerable<SymbolDisplayPart>
        {
            foreach (var part in parts) {
                writer.WriteSymbolDisplayPart(part, selected);
            }
        }

        public static void WriteSymbolDisplayPart(this FastUtf8JsonWriter writer, SymbolDisplayPart part, bool selected) {
            writer.WriteStartObject();
            writer.WriteProperty("text", part.ToString());
            writer.WriteProperty("kind", part.Kind.ToString("G").ToLowerInvariant());
            if (selected)
                writer.WriteProperty("selected", true);
            writer.WriteEndObject();
        }
    }
}
