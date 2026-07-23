using BenchmarkDotNet.Running;

namespace PerfBench
{
    class Program
    {
        static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run<DirectImageTransferBenchmark>();
        }
    }
}
