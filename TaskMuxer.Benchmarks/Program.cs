using BenchmarkDotNet.Running;
using TaskMuxer.Benchmarks;

BenchmarkRunner.Run<InstanceTaskMultiplexerSingleTypeBenchmarks>();
BenchmarkRunner.Run<InstanceTaskMultiplexerPolymorphicBenchmarks>();
