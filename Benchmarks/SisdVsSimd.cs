using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using BenchmarkDotNet;
using BenchmarkDotNet.Attributes;

namespace Benchmarks
{
    public class SisdVsSimd
    {
        const int Bailout = 5 * 1000 * 1000;

        private static readonly Complex[] Inputs = Enumerable.Repeat(new Complex(0,-0.5), Vector<float>.Count).ToArray();

        [Benchmark(Baseline = true)]
        public int Sisd()
        {
            int count = 0;
            foreach (var c in Inputs)
            {
                if (IsInSetDoubles(c.Real, c.Imaginary))
                {
                    count++;
                }
            }

            return count;
        }

        public bool IsInSetDoubles(double cReal, double cImag)
        {
            double zReal = 0;
            double zImag = 0;

            // Cache the squares
            // They are used to find the magnitude; reuse these values when computing the next re/im
            double zReal2 = 0;
            double zImag2 = 0;

            for (int i = 0; i < Bailout; i++)
            {
                double reTemp = zReal2 - zImag2 + cReal;
                zImag = 2 * zReal * zImag + cImag;
                zReal = reTemp;

                zReal2 = zReal * zReal;
                zImag2 = zImag * zImag;

                // Check the magnitude squared against 2^2
                if ((zReal2 + zImag2) > 4)
                {
                    return false;
                }
            }

            return true;
        }

        [Benchmark]
        public object Simd()
        {
            var cReal = new Vector<float>(Inputs.Select(c => (float)c.Real).ToArray());
            var cImag = new Vector<float>(Inputs.Select(c => (float)c.Imaginary).ToArray());

            var finalIterations = IsInSetVectorFloatExact(cReal, cImag);

            int count = 0;
            for (int i = 0; i < Vector<float>.Count; i++)
            {
                if (finalIterations[i] == Bailout)
                {
                    count++;
                }
            }
            return count;
        }

        public Vector<int> IsInSetVectorFloatExact(Vector<float> cReal, Vector<float> cImag)
        {
            var zReal = new Vector<float>(0);
            var zImag = new Vector<float>(0);

            var zReal2 = new Vector<float>(0);
            var zImag2 = new Vector<float>(0);

            var iterations = Vector<int>.Zero;
            var increment = Vector<int>.One;
            do
            {
                var realtemp = zReal2 - zImag2 + cReal;
                zImag = 2 * zReal * zImag + cImag;
                zReal = realtemp;

                zReal2 = zReal * zReal;
                zImag2 = zImag * zImag2;

                iterations += increment;
                var shouldContinue =
                    Vector.LessThanOrEqual(zReal2 + zImag2, new Vector<float>(4)) &
                    Vector.LessThanOrEqual(iterations, new Vector<int>(Bailout));
                increment = increment & shouldContinue;
            } while (increment != Vector<int>.Zero);

            return iterations;
        }

        public Vector<int> IsInSetVectorFloat(Vector<float> cReal, Vector<float> cImag)
        {
            var radiusSquared = new Vector<float>(4);
            var bailout = new Vector<int>(Bailout);

            var zReal = new Vector<float>(0);
            var zImag = new Vector<float>(0);

            var iterations = Vector<int>.Zero;
            var increment = Vector<int>.One;
            do
            {
                var naccumx = zReal * zReal - zImag * zImag;
                var naccumy = zReal * zImag + zReal * zImag;
                zReal = naccumx + cReal;
                zImag = naccumy + cImag;
                iterations += increment;
                var magnitudeSquared = zReal * zReal + zImag * zImag;
                var vCond =
                    Vector.LessThanOrEqual(magnitudeSquared, radiusSquared) &
                    Vector.LessThanOrEqual(iterations, bailout);
                increment = increment & vCond;
            } while (increment != Vector<int>.Zero);

            return iterations;
        }
    }
}
