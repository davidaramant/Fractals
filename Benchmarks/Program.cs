using System;
using System.Diagnostics;

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

            RunTest(() => $"* Scalar Parallel:\n\t{s.FindPointsScalarParallel()} points");

            //RunTest(() => $"* Vector early return:\n\t{s.FindPointsVectorsParallel()} points");
            RunTest(() => $"* Vector no early return:\n\t{s.FindPointsVectorsNoEarlyReturn()} points");
            RunTest(() => $"* OpenCL CPU:\n\t{s.FindPointsOpenClCpu()} points");
            RunTest(() => $"* OpenCL GPU:\n\t{s.FindPointsOpenClGpu()} points");

            //var summary = BenchmarkRunner.Run<ScalarVsVectorVsGpuPointFinder>();
        }
    }
}