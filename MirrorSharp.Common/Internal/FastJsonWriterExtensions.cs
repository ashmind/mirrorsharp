using Microsoft.CodeAnalysis.Text;

namespace MirrorSharp.Internal {
    public static class FastJsonWriterExtensions {
        public static void WriteSpan(this FastJsonWriter writer, TextSpan span) {
            writer.WriteStartObject();
            writer.WriteProperty("start", span.Start);
            writer.WriteProperty("length", span.Length);
            writer.WriteEndObject();
        }

        public static void WriteChange(this FastJsonWriter writer, TextChange change) {
            writer.WriteStartObject();
            writer.WriteProperty("start", change.Span.Start);
            writer.WriteProperty("length", change.Span.Length);
            writer.WriteProperty("text", change.NewText);
            writer.WriteEndObject();
        }
    }
}
