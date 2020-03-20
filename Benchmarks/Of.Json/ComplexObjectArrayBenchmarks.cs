using System.Text.Json;
using BenchmarkDotNet.Attributes;
using MirrorSharp.Internal;
using Newtonsoft.Json;

namespace MirrorSharp.Benchmarks.Of.Json {
    [InProcess]
    public class ComplexObjectArrayBenchmarks : JsonBenchmarksBase {
        /*
        {
            "type":"completions",
            "completions":{
                "span":{"start":70,"length":0},
                "list":[
                    {
                        "filterText":"Equals",
                        "displayText":"Equals",
                        "tags":["method","public"]
                    },
                    {
                        "filterText":"GetHashCode",
                        "displayText":"GetHashCode",
                        "tags":["method","public"]
                    },{
                        "filterText":"GetType",
                        "displayText":"GetType",
                        "tags":["method","public"]
                    },{
                        "filterText":"ToString",
                        "displayText":"ToString",
                        "tags":["method","public"]
                    }
                ]
            }
        }
        */

        private const int Operations = 1000;

        [Benchmark(OperationsPerInvoke = Operations)]
        public void NewtonsoftJson_JsonWriter() {
            var writer = _newtonsoftJsonWriter!;
            writer.WriteStartArray();
            for (var i = 0; i < Operations; i++) {
                writer.WriteStartObject();
                writer.WritePropertyName("type");
                writer.WriteValue("completions");
                writer.WritePropertyName("completions");
                writer.WriteStartObject();
                writer.WritePropertyName("span");
                writer.WriteStartObject();
                writer.WritePropertyName("start");
                writer.WriteValue(70);
                writer.WritePropertyName("length");
                writer.WriteValue(0);
                writer.WriteEndObject();
                writer.WritePropertyName("list");
                writer.WriteStartArray();
                WriteCompletion(writer, "Equals");
                WriteCompletion(writer, "GetHashCode");
                WriteCompletion(writer, "GetType");
                WriteCompletion(writer, "ToString");
                writer.WriteEndArray();
                writer.WriteEndObject();
                writer.WriteEndObject();
            }
            writer.WriteEndArray();
        }

        [Benchmark(OperationsPerInvoke = Operations)]
        public void MirrorSharp_FastJsonWriter() {
            var writer = _fastJsonWriter!;
            writer.WriteStartArray();
            for (var i = 0; i < Operations; i++) {
                writer.WriteStartObject();
                writer.WritePropertyName("type");
                writer.WriteValue("completions");
                writer.WritePropertyName("completions");
                writer.WriteStartObject();
                writer.WritePropertyName("span");
                writer.WriteStartObject();
                writer.WritePropertyName("start");
                writer.WriteValue(70);
                writer.WritePropertyName("length");
                writer.WriteValue(0);
                writer.WriteEndObject();
                writer.WritePropertyName("list");
                writer.WriteStartArray();
                WriteCompletion(writer, "Equals");
                WriteCompletion(writer, "GetHashCode");
                WriteCompletion(writer, "GetType");
                WriteCompletion(writer, "ToString");
                writer.WriteEndArray();
                writer.WriteEndObject();
                writer.WriteEndObject();
            }
            writer.WriteEndArray();
        }

        [Benchmark(OperationsPerInvoke = Operations)]
        public void SystemTextJson_Utf8JsonWriter() {
            var writer = _systemTextJsonWriter!;
            writer.WriteStartArray();
            for (var i = 0; i < Operations; i++) {
                writer.WriteStartObject();
                writer.WritePropertyName("type");
                writer.WriteStringValue("completions");
                writer.WritePropertyName("completions");
                writer.WriteStartObject();
                writer.WritePropertyName("span");
                writer.WriteStartObject();
                writer.WritePropertyName("start");
                writer.WriteNumberValue(70);
                writer.WritePropertyName("length");
                writer.WriteNumberValue(0);
                writer.WriteEndObject();
                writer.WritePropertyName("list");
                writer.WriteStartArray();
                WriteCompletion(writer, "Equals");
                WriteCompletion(writer, "GetHashCode");
                WriteCompletion(writer, "GetType");
                WriteCompletion(writer, "ToString");
                writer.WriteEndArray();
                writer.WriteEndObject();
                writer.WriteEndObject();
            }
            writer.WriteEndArray();
        }

        private static void WriteCompletion(JsonTextWriter writer, string text) {
            writer.WriteStartObject();
            writer.WritePropertyName("filterText");
            writer.WriteValue(text);
            writer.WritePropertyName("displayText");
            writer.WriteValue(text);
            writer.WritePropertyName("tags");
            writer.WriteStartArray();
            writer.WriteValue("method");
            writer.WriteValue("public");
            writer.WriteEndArray();
            writer.WriteEndObject();
        }

        private static void WriteCompletion(FastUtf8JsonWriter writer, string text) {
            writer.WriteStartObject();
            writer.WritePropertyName("filterText");
            writer.WriteValue(text);
            writer.WritePropertyName("displayText");
            writer.WriteValue(text);
            writer.WritePropertyName("tags");
            writer.WriteStartArray();
            writer.WriteValue("method");
            writer.WriteValue("public");
            writer.WriteEndArray();
            writer.WriteEndObject();
        }

        private static void WriteCompletion(Utf8JsonWriter writer, string text) {
            writer.WriteStartObject();
            writer.WritePropertyName("filterText");
            writer.WriteStringValue(text);
            writer.WritePropertyName("displayText");
            writer.WriteStringValue(text);
            writer.WritePropertyName("tags");
            writer.WriteStartArray();
            writer.WriteStringValue("method");
            writer.WriteStringValue("public");
            writer.WriteEndArray();
            writer.WriteEndObject();
        }
    }
}
