using BenchmarkDotNet.Running;
using MirrorSharp.Benchmarks.Of.Json;

namespace MirrorSharp.Benchmarks {
    public static class Program {
        public static void Main(string[] args) {
            BenchmarkRunner.Run<WriteValueStringBenchmarks>();
        }
    }
}
