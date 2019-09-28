using BenchmarkDotNet.Attributes;

namespace MirrorSharp.Benchmarks.Of.Json {
    [InProcess]
    public class WriteValueInt32Benchmarks : JsonBenchmarksBase {
        [Params(-100, -111, 1, 1111111)]
        public int Value { get; set; }

        [Benchmark]
        public void NewtonsoftJson_JsonWriter() {
            _newtonsoftJsonWriter!.WriteValue(Value);
            _newtonsoftJsonWriter.Flush();
        }

        [Benchmark]
        public void MirrorSharp_FastJsonWriter() {
            _fastJsonWriter!.WriteValue(Value);
        }

        [Benchmark]
        public void SystemTextJson_Utf8JsonWriter() {
            _systemTextJsonWriter!.WriteNumberValue(Value);
            _systemTextJsonWriter.Flush();
        }
    }
}
