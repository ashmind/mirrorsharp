using System;
using System.Buffers;
using System.IO;
using System.Text.Json;
using BenchmarkDotNet.Attributes;
using MirrorSharp.Internal;
using Newtonsoft.Json;

namespace MirrorSharp.Benchmarks.Of.Json {
    public class JsonBenchmarksBase {
        // ReSharper disable InconsistentNaming
        protected MemoryStream? _memoryStream;
        protected JsonTextWriter? _newtonsoftJsonWriter;
        internal FastUtf8JsonWriter? _fastJsonWriter;
        protected IBufferWriter<byte>? _bufferWriter;
        protected Utf8JsonWriter? _systemTextJsonWriter;
        // ReSharper restore InconsistentNaming

        [IterationSetup]
        public void Setup() {
            _memoryStream = new MemoryStream(4096);
            _newtonsoftJsonWriter = new JsonTextWriter(new StreamWriter(_memoryStream)) {
                Formatting = Formatting.None
            };
            _fastJsonWriter = new FastUtf8JsonWriter(ArrayPool<byte>.Create());

            _bufferWriter = new ArrayBufferWriter<byte>(4096);
            _systemTextJsonWriter = new Utf8JsonWriter(_bufferWriter, new JsonWriterOptions { Indented = false });
        }

        protected ArraySegment<byte> FlushNewtonsoftJsonWriterAndGetBuffer() {
            _newtonsoftJsonWriter!.Flush();
            ArraySegment<byte> buffer;
            _memoryStream!.TryGetBuffer(out buffer);
            return buffer;
        }

        protected ReadOnlyMemory<byte> FlushSystemTextJsonWriterAndGetBuffer() {
            _systemTextJsonWriter!.Flush();            
            return _bufferWriter!.GetMemory();
        }

        [IterationCleanup]
        public void Cleanup() {
            _fastJsonWriter!.Dispose();
            _systemTextJsonWriter!.Dispose();
        }
    }
}
