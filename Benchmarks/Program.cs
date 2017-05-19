using System;
using System.Diagnostics;
using NOpenCL;

namespace Benchmarks
{
    class Program
    {
        static void Main(string[] args)
        {
            // var summary = BenchmarkRunner.Run<ClassVsStructVsRawBenchmark>();
            // var summary = BenchmarkRunner.Run<BitmapVsFastImage>();

            var s = new ScalarVsVectorPointFinder();

            void RunTest(string name, Func<int> test)
            {
                Console.WriteLine($"* {name}:");
                const int NumberTrials = 3;
                var totals = new int[NumberTrials];

                GC.Collect();

                var stopwatch = Stopwatch.StartNew();
                for (int i = 0; i < NumberTrials; i++)
                {
                    s.InitializeRun();
                    totals[i] = test();
                }
                stopwatch.Stop();

                var pointsPerSecond = NumberTrials * ScalarVsVectorPointFinder.NumberOfPoints / stopwatch.Elapsed.TotalSeconds;
                Console.WriteLine($"\t{totals[0]} points (Took {stopwatch.Elapsed.TotalSeconds / NumberTrials:N1}s - {pointsPerSecond:N1} points/sec)");
            }

            Console.WriteLine(DateTime.Now.ToString("s"));
            Console.WriteLine($"Total Points: {ScalarVsVectorPointFinder.NumberOfPoints:N0}");
            Console.WriteLine($"Iteration Range: {s.Range}");

            //RunTest("Scalar", s.Scalar);
            //RunTest("Scalar Parallel", s.ScalarParallel);
            //RunTest("Scalar Parallel, No ADT", s.ScalarParallelNoAdt);
            //RunTest("Scalar Parallel, No ADT, Caching Squares", s.ScalarParallelNoAdtCachingSquares);
            //RunTest("Scalar Parallel, No ADT, Caching Squares, floats", s.ScalarParallelNoAdtCachingSquaresFloats);
            //RunTest("Scalar Parallel, No ADT, Caching Squares, Cycle Detection", s.ScalarParallelNoAdtCachingSquaresCycleDetection);

            //RunTest("Vectors, doubles", s.Vectors);
            //RunTest("Vectors, doubles, No Early Return", s.VectorsNoEarlyReturn);
            //RunTest("Vectors, floats", s.VectorsFloats);
            //RunTest("Vectors, floats, No Early Return", s.VectorsNoEarlyReturnFloats);

            using (s.SetupOpenCL(DeviceType.Gpu))
            {
                RunTest("OpenCL GPU, floats", s.OpenCLFloats);
            }

            //using (s.SetupOpenCL(DeviceType.Gpu, single: false))
            //{
            //    RunTest("OpenCL GPU, doubles", () => s.OpenCLDoubles());
            //}

            using (s.SetupOpenCL(DeviceType.Cpu))
            {
                RunTest("OpenCL CPU, floats", s.OpenCLFloats);
            }

            using (s.SetupOpenCL(DeviceType.Cpu, single: false))
            {
                RunTest("OpenCL CPU, doubles", s.OpenCLDoubles);
            }

            //var summary = BenchmarkRunner.Run<ScalarVsVectorVsGpuPointFinder>();
        }
    }
}