using System;
using System.IO;
using BenchmarkDotNet.Attributes;

namespace MirrorSharp.Benchmarks.Of.Json {
    public class WriteValueInt32Benchmarks : JsonBenchmarksBase {
        [Params(-111, 1, 1111111)]
        public int Value { get; set; }

        [Benchmark]
        public ArraySegment<byte> NewtonsoftJson_JsonWriter() {
            _memoryStream.Seek(0, SeekOrigin.Begin);
            _newtonsoftJsonWriter.WriteValue(Value);
            return FlushNewtonsoftJsonWriterAndGetBuffer();
        }

        [Benchmark]
        public ArraySegment<byte> MirrorSharp_FastJsonWriter() {
            _fastJsonWriter.Reset();
            _fastJsonWriter.WriteValue(Value);
            return _fastJsonWriter.WrittenSegment;
        }
    }
}
