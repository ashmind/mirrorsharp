using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BenchmarkDotNet.Running;
using MirrorSharp.Benchmarks.Of.Json;

namespace MirrorSharp.Benchmarks {
    public static class Program {
        public static void Main(string[] args) {
            BenchmarkRunner.Run<WriteValueStringBenchmarks>();
        }
    }
}
