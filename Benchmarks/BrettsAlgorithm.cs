using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Attributes.Exporters;
using Fractals.Model;
using Fractals.Utility;

namespace Benchmarks
{
    [HtmlExporter]
    public class BrettsAlgorithm
    {
        private static readonly IterationRange _range = new IterationRange(1_000_000, 5_000_000);

        private static readonly int NumInputs = Vector<float>.Count * 1000;

        [Benchmark(Baseline = true)]
        public int Scalar()
        {
            int count = 0;
            foreach (var c in CreateInputs())
            {
                if (IsInSetDoubles(c.Real, c.Imaginary, _range))
                {
                    count++;
                }
            }

            return count;
        }

        public bool IsInSetDoubles(double cReal, double cImag, IterationRange bailoutRange)
        {
            double zReal = 0;
            double zImag = 0;

            // Cache the squares
            // They are used to find the magnitude; reuse these values when computing the next re/im
            double zReal2 = 0;
            double zImag2 = 0;

            for (int i = 0; i < bailoutRange.ExclusiveMaximum; i++)
            {
                double reTemp = zReal2 - zImag2 + cReal;
                zImag = 2 * zReal * zImag + cImag;
                zReal = reTemp;

                zReal2 = zReal * zReal;
                zImag2 = zImag * zImag;

                // Check the magnitude squared against 2^2
                if ((zReal2 + zImag2) > 4)
                {
                    return i >= bailoutRange.InclusiveMinimum;
                }
            }

            return true;
        }

        [Benchmark]
        public int ScalarWithBrent()
        {
            int count = 0;
            foreach (var c in CreateInputs())
            {
                if (IsInSetDoublesWithBrent(c.Real, c.Imaginary, _range))
                {
                    count++;
                }
            }

            return count;
        }

        public static bool IsInSetDoublesWithBrent(double cReal, double cImag, IterationRange bailoutRange)
        {
            double re = 0;
            double im = 0;

            // Check for orbits
            // - Check re/im against an old point
            // - Only check every power of 2
            double oldRe = 0;
            double oldIm = 0;

            uint checkNum = 1;

            // Cache the squares
            // They are used to find the magnitude; reuse these values when computing the next re/im
            double re2 = 0;
            double im2 = 0;

            for (uint i = 0; i < bailoutRange.ExclusiveMaximum; i++)
            {
                var reTemp = re2 - im2 + cReal;
                im = 2 * re * im + cImag;
                re = reTemp;

                // Orbit check
                if (checkNum == i)
                {
                    // This is a safe comparison because in an orbit the points will literally be the same.
                    // If they differed at all, the error would compound upon iteration.
                    if (oldRe == re && oldIm == im)
                    {
                        return false;
                    }

                    oldRe = re;
                    oldIm = im;

                    checkNum = checkNum << 1;
                }

                re2 = re * re;
                im2 = im * im;

                // Check the magnitude squared against 2^2
                if ((re2 + im2) > 4)
                {
                    return i >= bailoutRange.InclusiveMinimum;
                }
            }

            return false;
        }

        private static IEnumerable<Complex> CreateInputs()
        {
            var area = new Area(
                realRange: new InclusiveRange(-1.999267578125, -1.99853515625),
                imagRange: new InclusiveRange(0, 0.000732421875));
            var rand = new Random(0);
            for (int i = 0; i < NumInputs; i++)
            {
                yield return area.GetRandomPoint(rand);
            }
        }
    }
}
