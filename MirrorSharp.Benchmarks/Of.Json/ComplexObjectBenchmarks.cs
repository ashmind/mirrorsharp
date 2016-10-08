using System;
using System.IO;
using BenchmarkDotNet.Attributes;
using MirrorSharp.Internal;
using Newtonsoft.Json;

namespace MirrorSharp.Benchmarks.Of.Json {
    public class ComplexObjectBenchmarks : JsonBenchmarksBase {
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

        [Benchmark]
        public ArraySegment<byte> NewtonsoftJson_JsonWriter() {
            _memoryStream.Seek(0, SeekOrigin.Begin);

            var writer = _newtonsoftJsonWriter;
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

            return FlushNewtonsoftJsonWriterAndGetBuffer();
        }

        [Benchmark]
        public ArraySegment<byte> MirrorSharp_FastJsonWriter() {
            _fastJsonWriter.Reset();

            var writer = _fastJsonWriter;
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

            return _fastJsonWriter.WrittenSegment;
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
    }
}
