using System;
using BenchmarkDotNet.Attributes;

namespace MirrorSharp.Benchmarks.Of.Json {
    [InProcess]
    public class WriteValueStringBenchmarks : JsonBenchmarksBase {
        [Params("test", @"using System;
            public class C {
                public void M() {
                }
            }
        ", "")]
        public string? Value { get; set; }

        [Benchmark]
        public void NewtonsoftJson_JsonWriter() {
            _newtonsoftJsonWriter!.WriteValue(Value);
            _newtonsoftJsonWriter.Flush();
            //return FlushNewtonsoftJsonWriterAndGetBuffer();
        }

        [Benchmark]
        public void MirrorSharp_FastJsonWriter() {
            _fastJsonWriter!.WriteValue(Value);
            //return _fastJsonWriter.WrittenSegment;
        }

        [Benchmark]
        public void SystemTextJson_Utf8JsonWriter() {
            _systemTextJsonWriter!.WriteStringValue(Value);
            _systemTextJsonWriter.Flush();
            //return FlushSystemTextJsonWriterAndGetBuffer();
        }
    }
}
