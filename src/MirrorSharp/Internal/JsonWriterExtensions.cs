using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

        public static void WriteStartArrayProperty(this JsonWriter writer, string name) {
            writer.WritePropertyName(name);
            writer.WriteStartArray();
        }

        public static void WriteStartObjectProperty(this JsonWriter writer, string name) {
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
