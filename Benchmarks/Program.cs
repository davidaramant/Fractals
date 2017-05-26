using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using NOpenCL;
using PowerArgs;

namespace Benchmarks
{
    class Program
    {
        public enum FloatPrecision
        {
            Single,
            Double
        }

        public class BenchmarkArgs
        {
            [HelpHook, ArgShortcut("-?"), ArgDescription("Shows this help")]
            public bool Help { get; set; }

            [ArgDescription("Number of points to use."), ArgDefaultValue(1024)]
            public int BatchSize { get; set; }

            [ArgDescription("Number of times to run each test."), ArgDefaultValue(5)]
            public int Trials { get; set; }

            [ArgDescription("Which floating point precision to test."), ArgDefaultValue(FloatPrecision.Single)]
            public FloatPrecision FloatPrecision { get; set; }

            [ArgDescription("For doubles, whether to only test with relaxed math."), ArgDefaultValue(false)]
            public bool OnlyDoRelaxedDoubleMath { get; set; }

            [ArgDescription("Dump the OpenCL info about the system.  If specified, no benchmarks are run."), ArgDefaultValue(false)]
            public bool GenerateSystemReport { get; set; }
        }

        static void Main(string[] args)
        {
            try
            {
                var parsedArgs = Args.Parse<BenchmarkArgs>(args);
                if (parsedArgs == null)
                    return;

                if (parsedArgs.GenerateSystemReport)
                {
                    LogDeviceInfo();
                    return;
                }

                // var summary = BenchmarkRunner.Run<ClassVsStructVsRawBenchmark>();
                // var summary = BenchmarkRunner.Run<BitmapVsFastImage>();

                var csvRows = new List<string>();
                string createRow(params object[] cols) => string.Join(",", cols.Select(c => $"\"{c.ToString().Replace("\"", "\"\"")}\""));

                var s = new IterationBenchmarks(parsedArgs.BatchSize, parsedArgs.Trials);

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

                //using (s.SetContext("Scalar CPU"))
                //{
                //    s.RunTest("No Threading", s.Scalar);
                //    s.RunTest("Baseline", s.ScalarParallel);
                //    s.RunTest("No ADT", s.ScalarParallelNoAdt);
                //    s.RunTest("No ADT, Caching Squares", s.ScalarParallelNoAdtCachingSquares);
                //    s.RunTest("No ADT, Caching Squares, floats", s.ScalarParallelNoAdtCachingSquaresFloats);
                //    s.RunTest("No ADT, Caching Squares, Cycle Detection", s.ScalarParallelNoAdtCachingSquaresCycleDetection);
                //}

                using (s.SetContext("Vector CPU - floats"))
                {
                    s.RunTest("Baseline", s.VectorsFloats);
                    s.RunTest("No Early Return", s.VectorsNoEarlyReturnFloats);
                    s.RunTest("No Early Return, Const Limit", s.VectorsNoEarlyReturnFloatsConstLimit);
                }

                using (s.SetContext("Vector CPU - doubles"))
                {
                    s.RunTest("Baseline", s.Vectors);
                    s.RunTest("No Early Return", s.VectorsNoEarlyReturn);
                    s.RunTest("No Early Return, Const Limit", s.VectorsNoEarlyReturnConstLimit);
                }

                //using (s.SetContext("OpenCL GPU - floats", openCLGuard: device => parsedArgs.FloatPrecision == FloatPrecision.Single))
                //{
                //    s.RunFloatOpenCLTest("Max Limit Argument",
                //        s.SetupOpenCL(DeviceType.Gpu, maxLimitArg: true, kernelName: "iterate_points_limit_argument"));

                //    s.RunFloatOpenCLTest("Baseline",
                //        s.SetupOpenCL(DeviceType.Gpu));

                //    //s.RunDoubleOpenCLTest("Early Return",
                //    //    s.SetupOpenCL(DeviceType.Gpu, kernelName: "iterate_points_early_return"));

                //    s.RunFloatOpenCLTest("Relaxed Math",
                //        s.SetupOpenCL(DeviceType.Gpu, relaxedMath: true));

                //    s.RunFloatOpenCLTest("FMA",
                //        s.SetupOpenCL(DeviceType.Gpu, kernelName: "iterate_points_fma"));

                //    s.RunFloatOpenCLTest("FMA, Relaxed Math",
                //        s.SetupOpenCL(DeviceType.Gpu, kernelName: "iterate_points_fma", relaxedMath: true));
                //}

                //using (s.SetContext("OpenCL GPU - doubles",
                //    openCLGuard: device => parsedArgs.FloatPrecision == FloatPrecision.Double && device.DoubleFloatingPointConfiguration != FloatingPointConfiguration.None))
                //{
                //    if (!parsedArgs.OnlyDoRelaxedDoubleMath)
                //    {
                //        s.RunDoubleOpenCLTest("Max Limit Argument",
                //            s.SetupOpenCL(DeviceType.Gpu, maxLimitArg: true,
                //                kernelName: "iterate_points_limit_argument", singlePrecision: false));

                //        s.RunDoubleOpenCLTest("Baseline",
                //            s.SetupOpenCL(DeviceType.Gpu, singlePrecision: false));

                //        //s.RunDoubleOpenCLTest("Early Return",
                //        //    s.SetupOpenCL(DeviceType.Gpu, singlePrecision: false,
                //        //        kernelName: "iterate_points_early_return"));
                //    }

                //    s.RunDoubleOpenCLTest("Relaxed Math",
                //        s.SetupOpenCL(DeviceType.Gpu, relaxedMath: true, singlePrecision: false));

                //    if (!parsedArgs.OnlyDoRelaxedDoubleMath)
                //    {
                //        s.RunDoubleOpenCLTest("FMA",
                //            s.SetupOpenCL(DeviceType.Gpu, kernelName: "iterate_points_fma", singlePrecision: false));
                //    }
                //}

                //using (s.SetContext("OpenCL CPU - floats"))
                //{
                //    s.RunFloatOpenCLTest("Max Limit Argument",
                //        s.SetupOpenCL(DeviceType.Cpu, maxLimitArg: true, kernelName: "iterate_points_limit_argument"));

                //    s.RunFloatOpenCLTest("Baseline",
                //        s.SetupOpenCL(DeviceType.Cpu));

                //    s.RunDoubleOpenCLTest("Early Return",
                //        s.SetupOpenCL(DeviceType.Cpu, kernelName: "iterate_points_early_return"));

                //    s.RunFloatOpenCLTest("Relaxed Math",
                //        s.SetupOpenCL(DeviceType.Cpu, relaxedMath: true));

                //    s.RunFloatOpenCLTest("FMA",
                //        s.SetupOpenCL(DeviceType.Cpu, kernelName: "iterate_points_fma"));

                //    s.RunFloatOpenCLTest("FMA, Relaxed Math",
                //        s.SetupOpenCL(DeviceType.Cpu, kernelName: "iterate_points_fma", relaxedMath: true));
                //}

                //using (s.SetContext("OpenCL CPU - doubles"))
                //{
                //    s.RunDoubleOpenCLTest("Max Limit Argument",
                //        s.SetupOpenCL(DeviceType.Cpu, maxLimitArg: true, kernelName: "iterate_points_limit_argument", singlePrecision: false));

                //    s.RunDoubleOpenCLTest("Baseline",
                //        s.SetupOpenCL(DeviceType.Cpu, singlePrecision: false));

                //    s.RunDoubleOpenCLTest("Early Return",
                //        s.SetupOpenCL(DeviceType.Cpu, singlePrecision: false, kernelName: "iterate_points_early_return"));

                //    s.RunDoubleOpenCLTest("Relaxed Math",
                //        s.SetupOpenCL(DeviceType.Cpu, relaxedMath: true, singlePrecision: false));

                //    s.RunDoubleOpenCLTest("FMA",
                //        s.SetupOpenCL(DeviceType.Cpu, kernelName: "iterate_points_fma", singlePrecision: false));

                //    s.RunDoubleOpenCLTest("FMA, Relaxed Math",
                //        s.SetupOpenCL(DeviceType.Cpu, kernelName: "iterate_points_fma", relaxedMath: true, singlePrecision: false));
                //}

                csvRows.Add(createRow("Context", "Test", "Points/Second", "Points Found"));
                foreach (var result in s.Results)
                {
                    csvRows.Add(createRow(result.Context, result.Name, result.Rate.ToString("N1"), result.PointsFound));
                }

                var csvPath = $"benchmarks.{Environment.MachineName}.{DateTime.Now.ToString("s").Replace(":", ".")}.csv";
                File.WriteAllLines(csvPath, csvRows);
                Console.WriteLine($"Wrote '{csvPath}'");
            }
            catch (ArgException e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(ArgUsage.GenerateUsageFromTemplate<BenchmarkArgs>());
            }
        }

        private static void LogDeviceInfo()
        {
            var path = $"OpenCL System Info - {Environment.MachineName}.txt";

            using (var fs = File.Open(path, FileMode.Create))
            using (var writer = new StreamWriter(fs))
            {
                foreach (var indexedPlatform in Platform.GetPlatforms().Select((p, index) => (p, index)))
                {
                    writer.WriteLine($"Platform {indexedPlatform.Item2}");
                    writer.WriteLine(JsonConvert.SerializeObject(indexedPlatform.Item1, Formatting.Indented, new Newtonsoft.Json.Converters.StringEnumConverter()));
                    writer.WriteLine();
                    foreach (var indexedDevice in indexedPlatform.Item1.GetDevices().Select((d, index) => (d, index)))
                    {
                        writer.WriteLine($"Platform {indexedPlatform.Item2}, Device {indexedDevice.Item2}");
                        writer.WriteLine(JsonConvert.SerializeObject(indexedDevice.Item1, Formatting.Indented, new JsonSerializerSettings
                        {
                            Error = (sender, eventArgs) => eventArgs.ErrorContext.Handled = true,
                            Converters = new JsonConverter[] { new Newtonsoft.Json.Converters.StringEnumConverter() }.ToList()
                        }));
                        writer.WriteLine();
                    }
                }
            }

            Console.Out.WriteLine($"Wrote \'{path}\'");
        }
    }
}