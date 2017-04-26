using System;
using System.Diagnostics;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;
using Fractals.Utility;

namespace Benchmarks
{
    class Program
    {
        static void Main(string[] args)
        {
            // var summary = BenchmarkRunner.Run<ClassVsStructVsRawBenchmark>();
            // var summary = BenchmarkRunner.Run<BitmapVsFastImage>();

            var s = new ScalarVsVectorVsGpuPointFinder();

            void RunTest(Func<string> test)
            {
                GC.Collect();

                s.Initialize();
                var stopwatch = Stopwatch.StartNew();
                Console.Write(test());
                stopwatch.Stop();
                Console.WriteLine($" (Took {stopwatch.ElapsedMilliseconds}ms)");
                s.Cleanup();
            }

            //RunTest(() => $"Vector early return: {s.FindPointsVectorsParallel()} points");
            //RunTest(() => $"Vector no early return: {s.FindPointsVectorsNoEarlyReturn()} points");
            RunTest(() => $"OpenCL CPU: {s.FindPointsOpenClCpu()} points");
            RunTest(() => $"OpenCL GPU: {s.FindPointsOpenClGpu()} points");

            //var summary = BenchmarkRunner.Run<ScalarVsVectorVsGpuPointFinder>();
        }
    }
}
