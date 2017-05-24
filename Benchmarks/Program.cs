using System;
using System.Collections.Generic;
using System.Linq;
using NOpenCL;

namespace Benchmarks
{
    class Program
    {

        static void Main(string[] args)
        {
            // var summary = BenchmarkRunner.Run<ClassVsStructVsRawBenchmark>();
            // var summary = BenchmarkRunner.Run<BitmapVsFastImage>();

            var csvRows = new List<string>();
            string createRow(params object[] cols) => string.Join(",", cols.Select(c => $"\"{c.ToString().Replace("\"", "\"\"")}\""));

            var s = new IterationBenchmarks();

            Console.WriteLine(DateTime.Now.ToString("s"));
            Console.WriteLine($"Total Points: {s.NumberOfPoints:N0}");
            Console.WriteLine($"Iteration Range: {s.Range}");
            Console.WriteLine($"Trials: {s.NumberTrials}");

            csvRows.Add(createRow("Timestamp", DateTime.Now.ToString("s")));
            csvRows.Add(createRow("Total Points", s.NumberOfPoints.ToString("N0")));
            csvRows.Add(createRow("Iteration Min", s.Range.InclusiveMinimum));
            csvRows.Add(createRow("Iteration Max", s.Range.ExclusiveMaximum));
            csvRows.Add(createRow("Trials", s.NumberTrials));
            csvRows.Add(createRow("Machine", Environment.MachineName));
            csvRows.Add(string.Empty);

            using (s.SetContext("Scalar CPU"))
            {
                s.RunTest("No Threading", s.Scalar);
                s.RunTest("Baseline", s.ScalarParallel);
                s.RunTest("No ADT", s.ScalarParallelNoAdt);
                s.RunTest("No ADT, Caching Squares", s.ScalarParallelNoAdtCachingSquares);
                s.RunTest("No ADT, Caching Squares, floats", s.ScalarParallelNoAdtCachingSquaresFloats);
                s.RunTest("No ADT, Caching Squares, Cycle Detection", s.ScalarParallelNoAdtCachingSquaresCycleDetection);
            }

            using (s.SetContext("Vector CPU - floats"))
            {
                s.RunTest("Baseline", s.VectorsFloats);
                s.RunTest("No Early Return", s.VectorsNoEarlyReturnFloats);
            }

            using (s.SetContext("Vector CPU - doubles"))
            {
                s.RunTest("Baseline", s.Vectors);
                s.RunTest("No Early Return", s.VectorsNoEarlyReturn);
            }

            using (s.SetContext("OpenCL GPU - floats"))
            {
                s.RunFloatOpenCLTest("Max Limit Argument",
                    s.SetupOpenCL(DeviceType.Gpu, maxLimitArg: true, kernelName: "iterate_points_limit_argument"));

                s.RunFloatOpenCLTest("Baseline",
                    s.SetupOpenCL(DeviceType.Gpu));

                s.RunDoubleOpenCLTest("Early Return",
                    s.SetupOpenCL(DeviceType.Gpu, kernelName: "iterate_points_early_return"));

                s.RunFloatOpenCLTest("Relaxed Math",
                    s.SetupOpenCL(DeviceType.Gpu, relaxedMath: true));

                s.RunFloatOpenCLTest("FMA",
                    s.SetupOpenCL(DeviceType.Gpu, kernelName: "iterate_points_fma"));

                s.RunFloatOpenCLTest("FMA, Relaxed Math",
                    s.SetupOpenCL(DeviceType.Gpu, kernelName: "iterate_points_fma", relaxedMath: true));
            }

            using (s.SetContext("OpenCL GPU - doubles", 
                openCLGuard:device=>device.DoubleFloatingPointConfiguration != FloatingPointConfiguration.None))
            {
                s.RunDoubleOpenCLTest("Max Limit Argument",
                    s.SetupOpenCL(DeviceType.Gpu, maxLimitArg: true, kernelName: "iterate_points_limit_argument", singlePrecision: false));

                s.RunDoubleOpenCLTest("Baseline",
                    s.SetupOpenCL(DeviceType.Gpu, singlePrecision: false));

                s.RunDoubleOpenCLTest("Early Return",
                    s.SetupOpenCL(DeviceType.Gpu, singlePrecision: false, kernelName: "iterate_points_early_return"));

                s.RunDoubleOpenCLTest("Relaxed Math",
                    s.SetupOpenCL(DeviceType.Gpu, relaxedMath: true, singlePrecision: false));

                s.RunDoubleOpenCLTest("FMA",
                    s.SetupOpenCL(DeviceType.Gpu, kernelName: "iterate_points_fma", singlePrecision: false));

                s.RunDoubleOpenCLTest("FMA, Relaxed Math",
                    s.SetupOpenCL(DeviceType.Gpu, kernelName: "iterate_points_fma", relaxedMath: true, singlePrecision: false));
            }

            using (s.SetContext("OpenCL CPU - floats"))
            {
                s.RunFloatOpenCLTest("Max Limit Argument",
                    s.SetupOpenCL(DeviceType.Cpu, maxLimitArg: true, kernelName: "iterate_points_limit_argument"));

                s.RunFloatOpenCLTest("Baseline",
                    s.SetupOpenCL(DeviceType.Cpu));

                s.RunDoubleOpenCLTest("Early Return",
                    s.SetupOpenCL(DeviceType.Cpu, kernelName: "iterate_points_early_return"));

                s.RunFloatOpenCLTest("Relaxed Math",
                    s.SetupOpenCL(DeviceType.Cpu, relaxedMath: true));

                s.RunFloatOpenCLTest("FMA",
                    s.SetupOpenCL(DeviceType.Cpu, kernelName: "iterate_points_fma"));

                s.RunFloatOpenCLTest("FMA, Relaxed Math",
                    s.SetupOpenCL(DeviceType.Cpu, kernelName: "iterate_points_fma", relaxedMath: true));
            }

            using (s.SetContext("OpenCL CPU - doubles"))
            {
                s.RunDoubleOpenCLTest("Max Limit Argument",
                    s.SetupOpenCL(DeviceType.Cpu, maxLimitArg: true, kernelName: "iterate_points_limit_argument", singlePrecision: false));

                s.RunDoubleOpenCLTest("Baseline",
                    s.SetupOpenCL(DeviceType.Cpu, singlePrecision: false));

                s.RunDoubleOpenCLTest("Early Return",
                    s.SetupOpenCL(DeviceType.Cpu, singlePrecision: false, kernelName: "iterate_points_early_return"));

                s.RunDoubleOpenCLTest("Relaxed Math",
                    s.SetupOpenCL(DeviceType.Cpu, relaxedMath: true, singlePrecision: false));

                s.RunDoubleOpenCLTest("FMA",
                    s.SetupOpenCL(DeviceType.Cpu, kernelName: "iterate_points_fma", singlePrecision: false));

                s.RunDoubleOpenCLTest("FMA, Relaxed Math",
                    s.SetupOpenCL(DeviceType.Cpu, kernelName: "iterate_points_fma", relaxedMath: true, singlePrecision: false));
            }

            csvRows.Add(createRow("Context", "Test", "Points/Second", "Points Found"));
            foreach (var result in s.Results)
            {
                csvRows.Add(createRow(result.Context, result.Name, result.Rate.ToString("N1"), result.PointsFound));
            }

            System.IO.File.WriteAllLines(Environment.MachineName + ".benchmarks.csv", csvRows);
        }
    }
}