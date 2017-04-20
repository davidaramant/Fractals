using System;
using BenchmarkDotNet.Running;

namespace Benchmarks
{
    class Program
    {
        static void Main(string[] args)
        {
            // var summary = BenchmarkRunner.Run<ClassVsStructVsRawBenchmark>();
            // var summary = BenchmarkRunner.Run<BitmapVsFastImage>();

            //var s = new ScalarVsVectorPointFinder();
            //s.InitializePointGenerator();
            //var scalarPoints = s.FindPointsScalarParallel();
            //s.InitializePointGenerator();
            //var vectorPoints = s.FindPointsVectorsParallel();

            //Console.WriteLine($"scalar: {scalarPoints}, vector: {vectorPoints}");
            //return;

            var summary = BenchmarkRunner.Run<ScalarVsVectorPointFinder>();
            // TODO: Brett's algorithm
            // TODO: GPU

            //var summary = BenchmarkRunner.Run<RandomPointGeneration>();

        }
    }
}
