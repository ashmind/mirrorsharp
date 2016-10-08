using System;
using System.IO;
using BenchmarkDotNet.Attributes;

namespace MirrorSharp.Benchmarks.Of.Json {
    public class WriteValueStringBenchmarks : JsonBenchmarksBase {
        private const string String = @"
            using System;
            public class C {
                public void M() {
                }
            }
        ";

        [Benchmark]
        public ArraySegment<byte> NewtonsoftJson_JsonWriter() {
            _memoryStream.Seek(0, SeekOrigin.Begin);
            _newtonsoftJsonWriter.WriteValue(String);
            return FlushNewtonsoftJsonWriterAndGetBuffer();
        }

        [Benchmark]
        public ArraySegment<byte> MirrorSharp_FastJsonWriter() {
            _fastJsonWriter.Reset();
            _fastJsonWriter.WriteValue(String);
            return _fastJsonWriter.WrittenSegment;
        }
    }
}
