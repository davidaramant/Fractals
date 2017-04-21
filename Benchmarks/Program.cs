using System;
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

            //var s = new ScalarVsVectorVsGpuPointFinder();

            //s.Initialize();
            //Console.Out.WriteLine($"scalar points: {s.FindPointsScalarParallel()}");
            //s.Cleanup();

            //s.Initialize();
            //Console.Out.WriteLine($"vector points: {s.FindPointsVectorsParallel()}");
            //s.Cleanup();

            //s.Initialize();
            //Console.Out.WriteLine($"vector points (no early return): {s.FindPointsVectorsParallelNoEarlyReturn()}");
            //s.Cleanup();

            ////s.Initialize();
            ////Console.WriteLine($"gpu points: {s.FindPointsGpu()}");
            ////s.Cleanup();

            //return;

            var summary = BenchmarkRunner.Run<ScalarVsVectorVsGpuPointFinder>();

            //var summary = BenchmarkRunner.Run<RandomPointGeneration>();

        }
    }
}
