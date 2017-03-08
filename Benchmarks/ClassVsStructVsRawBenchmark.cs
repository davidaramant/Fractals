using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Attributes.Exporters;

namespace Benchmarks
{
    [HtmlExporter]
    public class ClassVsStructVsRawBenchmark
    {
        const int Bailout = 5 * 1000 * 1000;
       
        [Benchmark(Baseline = true)]
        public bool RawFloats()
        {
            float cReal = -0.5f;
            float cImag = 0f;

            float zReal = 0;
            float zImag = 0;

            // Cache the squares
            // They are used to find the magnitude; reuse these values when computing the next re/im
            float zReal2 = 0;
            float zImag2 = 0;

            for (int i = 0; i < Bailout; i++)
            {
                float reTemp = zReal2 - zImag2 + cReal;
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
        public bool RawDoubles()
        {
            double cReal = -0.5;
            double cImag = 0;

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
        public bool NumericsComplex()
        {
            var c = new Complex(-0.5,0);

            var z = new Complex();

            for (int i = 0; i < Bailout; i++)
            {
                z = z * z + c;

                // Check the magnitude squared against 2^2
                if ((z.Real*z.Real + z.Imaginary*z.Imaginary) > 4)
                {
                    return false;
                }
            }

            return true;
        }

        public sealed class ComplexClass
        {
            public readonly float Real;
            public readonly float Imag;

            public ComplexClass(float re, float im)
            {
                Real = re;
                Imag = im;
            }

            public static ComplexClass operator +(ComplexClass left, ComplexClass right)
            {
                return (new ComplexClass((left.Real + right.Real), (left.Imag + right.Imag)));
            }

            public static ComplexClass operator *(ComplexClass left, ComplexClass right)
            {
                // Multiplication:  (a + bi)(c + di) = (ac -bd) + (bc + ad)i
                var result_real = (left.Real * right.Real) - (left.Imag * right.Imag);
                var result_imag = (left.Imag * right.Real) + (left.Real * right.Imag);
                return (new ComplexClass(result_real, result_imag));
            }

            public float MagnitureSquared => Real * Real + Imag * Imag;
        }

        [Benchmark]
        public bool CustomComplexClass()
        {
            var c = new ComplexClass(-0.5f, 0);

            var z = new ComplexClass(0,0);

            for (int i = 0; i < Bailout; i++)
            {
                z = z * z + c;

                // Check the magnitude squared against 2^2
                if (z.MagnitureSquared > 4)
                {
                    return false;
                }
            }

            return true;
        }

        public struct ComplexStruct
        {
            public readonly float Real;
            public readonly float Imag;

            public ComplexStruct(float re, float im)
            {
                Real = re;
                Imag = im;
            }

            public static ComplexStruct operator +(ComplexStruct left, ComplexStruct right)
            {
                return (new ComplexStruct((left.Real + right.Real), (left.Imag + right.Imag)));
            }

            public static ComplexStruct operator *(ComplexStruct left, ComplexStruct right)
            {
                // Multiplication:  (a + bi)(c + di) = (ac -bd) + (bc + ad)i
                var result_real = (left.Real * right.Real) - (left.Imag * right.Imag);
                var result_imag = (left.Imag * right.Real) + (left.Real * right.Imag);
                return (new ComplexStruct(result_real, result_imag));
            }

            public float MagnitureSquared => Real * Real + Imag * Imag;
        }

        [Benchmark]
        public bool CustomComplexStruct()
        {
            var c = new ComplexStruct(-0.5f, 0);

            var z = new ComplexStruct(0, 0);

            for (int i = 0; i < Bailout; i++)
            {
                z = z * z + c;

                // Check the magnitude squared against 2^2
                if (z.MagnitureSquared > 4)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
