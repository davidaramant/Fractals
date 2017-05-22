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

            const int NumberTrials = 3;

            void RunTest(string name, Func<int> test)
            {
                Console.WriteLine($"* {name}:");

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

            void RunFloatOpenCLTest(string name, IDisposable setup)
            {
                using (setup)
                {
                    RunTest(name, s.OpenCLFloats);
                }
            }

            void RunDoubleOpenCLTest(string name, IDisposable setup)
            {
                using (setup)
                {
                    RunTest(name, s.OpenCLDoubles);
                }
            }

            Console.WriteLine(DateTime.Now.ToString("s"));
            Console.WriteLine($"Total Points: {ScalarVsVectorPointFinder.NumberOfPoints:N0}");
            Console.WriteLine($"Iteration Range: {s.Range}");
            Console.WriteLine($"Trials: {NumberTrials}");

            void SectionHeader(string name) => Console.WriteLine($"\n\n{new string('#', name.Length)}\n{name}\n{new string('#', name.Length)}\n");

            //SectionHeader("Scalar CPU");
            //RunTest("Scalar", s.Scalar);
            //RunTest("Scalar Parallel", s.ScalarParallel);
            //RunTest("Scalar Parallel, No ADT", s.ScalarParallelNoAdt);
            //RunTest("Scalar Parallel, No ADT, Caching Squares", s.ScalarParallelNoAdtCachingSquares);
            //RunTest("Scalar Parallel, No ADT, Caching Squares, floats", s.ScalarParallelNoAdtCachingSquaresFloats);
            //RunTest("Scalar Parallel, No ADT, Caching Squares, Cycle Detection", s.ScalarParallelNoAdtCachingSquaresCycleDetection);

            //SectionHeader("Vector CPU");
            //RunTest("Vectors, doubles", s.Vectors);
            //RunTest("Vectors, doubles, No Early Return", s.VectorsNoEarlyReturn);
            //RunTest("Vectors, floats", s.VectorsFloats);
            //RunTest("Vectors, floats, No Early Return", s.VectorsNoEarlyReturnFloats);

            SectionHeader("OpenCL GPU - floats");

            RunFloatOpenCLTest("Max Limit Argument",
                s.SetupOpenCL(DeviceType.Gpu, maxLimitArg: true, kernelName: "iterate_points_limit_argument"));

            RunFloatOpenCLTest("Baseline",
                s.SetupOpenCL(DeviceType.Gpu));

            RunFloatOpenCLTest("Relaxed Math",
                s.SetupOpenCL(DeviceType.Gpu, relaxedMath: true));

            RunFloatOpenCLTest("FMA",
                s.SetupOpenCL(DeviceType.Gpu, kernelName: "iterate_points_fma"));

            RunFloatOpenCLTest("FMA, Relaxed Math",
                s.SetupOpenCL(DeviceType.Gpu, kernelName: "iterate_points_fma", relaxedMath: true));


            //SectionHeader("OpenCL GPU - doubles");

            //RunDoubleOpenCLTest("Max Limit Argument",
            //    s.SetupOpenCL(DeviceType.Gpu, maxLimitArg: true, kernelName: "iterate_points_limit_argument", singlePrecision: false));

            //RunDoubleOpenCLTest("Baseline",
            //    s.SetupOpenCL(DeviceType.Gpu, singlePrecision: false));

            //RunDoubleOpenCLTest("Relaxed Math",
            //    s.SetupOpenCL(DeviceType.Gpu, relaxedMath: true, singlePrecision: false));

            //RunDoubleOpenCLTest("FMA",
            //    s.SetupOpenCL(DeviceType.Gpu, kernelName: "iterate_points_fma", singlePrecision: false));

            //RunDoubleOpenCLTest("FMA, Relaxed Math",
            //    s.SetupOpenCL(DeviceType.Gpu, kernelName: "iterate_points_fma", relaxedMath: true, singlePrecision: false));


            SectionHeader("OpenCL CPU - floats");

            RunFloatOpenCLTest("Max Limit Argument",
                s.SetupOpenCL(DeviceType.Cpu, maxLimitArg: true, kernelName: "iterate_points_limit_argument"));

            RunFloatOpenCLTest("Baseline",
                s.SetupOpenCL(DeviceType.Cpu));

            RunFloatOpenCLTest("Relaxed Math",
                s.SetupOpenCL(DeviceType.Cpu, relaxedMath: true));

            RunFloatOpenCLTest("FMA",
                s.SetupOpenCL(DeviceType.Cpu, kernelName: "iterate_points_fma"));

            RunFloatOpenCLTest("FMA, Relaxed Math",
                s.SetupOpenCL(DeviceType.Cpu, kernelName: "iterate_points_fma", relaxedMath: true));


            SectionHeader("OpenCL CPU - doubles");

            RunDoubleOpenCLTest("Max Limit Argument",
                s.SetupOpenCL(DeviceType.Cpu, maxLimitArg: true, kernelName: "iterate_points_limit_argument", singlePrecision: false));

            RunDoubleOpenCLTest("Baseline",
                s.SetupOpenCL(DeviceType.Cpu, singlePrecision: false));

            RunDoubleOpenCLTest("Relaxed Math",
                s.SetupOpenCL(DeviceType.Cpu, relaxedMath: true, singlePrecision: false));

            RunDoubleOpenCLTest("FMA",
                s.SetupOpenCL(DeviceType.Cpu, kernelName: "iterate_points_fma", singlePrecision: false));

            RunDoubleOpenCLTest("FMA, Relaxed Math",
                s.SetupOpenCL(DeviceType.Cpu, kernelName: "iterate_points_fma", relaxedMath: true, singlePrecision: false));



            //var summary = BenchmarkRunner.Run<ScalarVsVectorVsGpuPointFinder>();
        }
    }
}