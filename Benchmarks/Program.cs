﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BenchmarkDotNet.Running;

namespace Benchmarks
{
    class Program
    {
        static void Main(string[] args)
        {
//            var summary = BenchmarkRunner.Run<ClassVsStructVsRawBenchmark>();
            var summary = BenchmarkRunner.Run<BitmapVsFastImage>();
        }
    }
}
