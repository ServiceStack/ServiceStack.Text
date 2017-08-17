using System;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Validators;

namespace ServiceStack.Text.Benchmarks
{
    public class Program
    {
        public static void Main(string[] args)
        {
            //BenchmarkRunner.Run<JsonSerializationBenchmarks>(
            BenchmarkRunner.Run<ParseBuiltinBenchmarks>(
              ManualConfig
                .Create(DefaultConfig.Instance)
                //.With(Job.RyuJitX64)
                .With(Job.Core)
                .With(new BenchmarkDotNet.Diagnosers.CompositeDiagnoser())
                .With(ExecutionValidator.FailOnError)
            );
        }
    }
}
