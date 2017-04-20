using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using BenchmarkDotNet.Running;

namespace Benchmarks
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine($"Number of floats in vector: {Vector<float>.Count}");
            Console.WriteLine($"Number of ints in vector: {Vector<int>.Count}");

            // var summary = BenchmarkRunner.Run<ClassVsStructVsRawBenchmark>();
            // var summary = BenchmarkRunner.Run<BitmapVsFastImage>();

            //var s = new ScalarVsVectorPointFinder();
            //s.InitializePointGenerator();
            //var scalarPoints = s.FindPointsScalar();
            //s.InitializePointGenerator();
            //var vectorPoints = s.FindPointsVectors();

            //Console.WriteLine($"scalar: {scalarPoints}, vector: {vectorPoints}");
            //return;

            var summary = BenchmarkRunner.Run<ScalarVsVectorPointFinder>();
            // TODO: Brett's algorithm
            // TODO: GPU

            //var summary = BenchmarkRunner.Run<RandomPointGeneration>();

        }
    }
}
