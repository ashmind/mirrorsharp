using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using MirrorSharp.Benchmarks.Of.Json;
using MirrorSharp.Internal;
using Newtonsoft.Json;

namespace MirrorSharp.Benchmarks {
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
