using System;
using System.Buffers;
using System.IO;
using BenchmarkDotNet.Attributes;
using MirrorSharp.Internal;
using Newtonsoft.Json;

namespace MirrorSharp.Benchmarks.Of.Json {
    public class JsonBenchmarksBase {
        // ReSharper disable InconsistentNaming
        protected MemoryStream _memoryStream;
        protected JsonTextWriter _newtonsoftJsonWriter;
        internal FastUtf8JsonWriter _fastJsonWriter;
        // ReSharper restore InconsistentNaming

        [Setup]
        public void Setup() {
            _memoryStream = new MemoryStream();
            _newtonsoftJsonWriter = new JsonTextWriter(new StreamWriter(_memoryStream)) {
                Formatting = Formatting.None
            };
            _fastJsonWriter = new FastUtf8JsonWriter(ArrayPool<byte>.Shared);
        }

        protected ArraySegment<byte> FlushNewtonsoftJsonWriterAndGetBuffer() {
            _newtonsoftJsonWriter.Flush();
            ArraySegment<byte> buffer;
            _memoryStream.TryGetBuffer(out buffer);
            return buffer;
        }

        [Cleanup]
        public void Cleanup() {
            _fastJsonWriter.Dispose();
        }
    }
}
