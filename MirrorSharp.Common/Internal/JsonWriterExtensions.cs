using Microsoft.CodeAnalysis.Text;
using Newtonsoft.Json;

namespace MirrorSharp.Internal {
    public static class JsonWriterExtensions {
        public static void WriteProperty(this JsonWriter writer, string name, int value) {
            writer.WritePropertyName(name);
            writer.WriteValue(value);
        }

        public static void WriteProperty(this JsonWriter writer, string name, int? value) {
            writer.WritePropertyName(name);
            writer.WriteValue(value);
        }

        public static void WriteProperty(this JsonWriter writer, string name, string value) {
            writer.WritePropertyName(name);
            writer.WriteValue(value);
        }

        public static void WritePropertyStartArray(this JsonWriter writer, string name) {
            writer.WritePropertyName(name);
            writer.WriteStartArray();
        }

        public static void WritePropertyStartObject(this JsonWriter writer, string name) {
            writer.WritePropertyName(name);
            writer.WriteStartObject();
        }

        public static void WriteSpan(this JsonWriter writer, TextSpan span) {
            writer.WriteStartObject();
            writer.WriteProperty("start", span.Start);
            writer.WriteProperty("length", span.Length);
            writer.WriteEndObject();
        }
    }
}
