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
            Console.Out.WriteLine($"Number of floats in vector: {Vector<float>.Count}");

            // var summary = BenchmarkRunner.Run<ClassVsStructVsRawBenchmark>();
            // var summary = BenchmarkRunner.Run<BitmapVsFastImage>();
            var summary = BenchmarkRunner.Run<SisdVsSimd>();
            // TODO: Brett's algorithm
            // TODO: GPU

        }
    }
}
