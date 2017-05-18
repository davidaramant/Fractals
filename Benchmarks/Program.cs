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

            void RunTest(string name, Func<int> test)
            {
                Console.WriteLine($"* {name}:");
                const int NumberTrials = 3;
                var totals = new int[NumberTrials];

                GC.Collect();

                s.Initialize();
                var stopwatch = Stopwatch.StartNew();
                for (int i = 0; i < NumberTrials; i++)
                {
                    totals[i] = test();
                }
                stopwatch.Stop();

                var pointsPerSecond = 3 * ScalarVsVectorVsGpuPointFinder.NumberOfPoints / stopwatch.Elapsed.TotalSeconds;
                Console.WriteLine($"\t{totals[0]} points (Took {stopwatch.Elapsed.TotalSeconds / NumberTrials:N1}s - {pointsPerSecond:N1} points/sec)");
            }

            Console.WriteLine(DateTime.Now.ToString("s"));
            Console.WriteLine($"Total Points: {ScalarVsVectorVsGpuPointFinder.NumberOfPoints:N0}");
            Console.WriteLine($"Iteration Range: {s.Range}");

            //RunTest("Scalar", s.Scalar);
            //RunTest("Scalar Parallel", s.ScalarParallel);
            //RunTest("Scalar Parallel, No ADT", s.ScalarParallelNoAdt);
            //RunTest("Scalar Parallel, No ADT, Caching Squares", s.ScalarParallelNoAdtCachingSquares);
            //RunTest("Scalar Parallel, No ADT, Caching Squares, Floats", s.ScalarParallelNoAdtCachingSquaresFloats);
            //RunTest("Scalar Parallel, No ADT, Caching Squares, Cycle Detection", s.ScalarParallelNoAdtCachingSquaresCycleDetection);

            //RunTest("Vectors", s.Vectors);
            //RunTest("Vectors, No Early Return", s.VectorsNoEarlyReturn);
            RunTest("Vectors, floats", s.VectorsFloats);
            RunTest("Vectors, floats, No Early Return", s.VectorsNoEarlyReturnFloats);


            //RunTest(() => $"* Scalar Parallel:\n\t{s.FindPointsScalarParallel()} points");
            //RunTest(() => $"* Vector early return:\n\t{s.FindPointsVectorsParallel()} points");
            //RunTest(() => $"* Vector no early return:\n\t{s.FindPointsVectorsNoEarlyReturn()} points");
            //RunTest(() => $"* OpenCL CPU:\n\t{s.FindPointsOpenClCpu()} points");
            //RunTest(() => $"* OpenCL GPU:\n\t{s.FindPointsOpenClGpu()} points");
            //RunTest(() => $"* OpenCL Heterogenous:\n\t{s.FindPointsOpenClHeterogenous(cpuRatio:52,gpuRatio:46)} points");

            //var summary = BenchmarkRunner.Run<ScalarVsVectorVsGpuPointFinder>();
        }
    }
}