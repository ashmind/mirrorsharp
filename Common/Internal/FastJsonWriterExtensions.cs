using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using MirrorSharp.Advanced;

namespace MirrorSharp.Internal {
    internal static class FastJsonWriterExtensions {
        public static void WriteSpan(this IFastJsonWriter writer, TextSpan span) {
            writer.WriteStartObject();
            writer.WriteProperty("start", span.Start);
            writer.WriteProperty("length", span.Length);
            writer.WriteEndObject();
        }

        public static void WriteChange(this IFastJsonWriter writer, TextChange change) {
            writer.WriteStartObject();
            writer.WriteProperty("start", change.Span.Start);
            writer.WriteProperty("length", change.Span.Length);
            writer.WriteProperty("text", change.NewText);
            writer.WriteEndObject();
        }

        public static void WriteSymbolDisplayParts<TCollection>(this IFastJsonWriter writer, TCollection parts, bool selected = false)
            where TCollection : IEnumerable<SymbolDisplayPart>
        {
            foreach (var part in parts) {
                writer.WriteSymbolDisplayPart(part, selected);
            }
        }

        public static void WriteSymbolDisplayPart(this IFastJsonWriter writer, SymbolDisplayPart part, bool selected) {
            writer.WriteStartObject();
            writer.WriteProperty("text", part.ToString());
            writer.WriteProperty("kind", FastConvert.EnumToLowerInvariantString(part.Kind));
            if (selected)
                writer.WriteProperty("selected", true);
            writer.WriteEndObject();
        }

        public static void WriteTaggedTexts<TCollection>(this IFastJsonWriter writer, TCollection texts, bool selected = false)
            where TCollection : IEnumerable<TaggedText>
        {
            foreach (var text in texts) {
                writer.WriteTaggedText(text, selected);
            }
        }

        public static void WriteTaggedText(this IFastJsonWriter writer, TaggedText text, bool selected) {
            writer.WriteStartObject();
            writer.WriteProperty("text", text.Text);
            writer.WriteProperty("kind", text.Tag);
            if (selected)
                writer.WriteProperty("selected", true);
            writer.WriteEndObject();
        }
    }
}
