using System;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using Fractals.Model;
using Fractals.PointGenerator;

namespace Fractals.Utility
{
    public class BuddhabrotPointFinder : PointFinder
    {
        public BuddhabrotPointFinder(uint minimum, uint maximum, string outputDirectory, string outputFile, IRandomPointGenerator pointGenerator)
            : base(minimum, maximum, outputDirectory, outputFile, pointGenerator)
        {
        }

        protected override bool ValidatePoint(Complex c, BailoutRange bailoutRange)
        {
            return IsPointInBuddhabrot(c, bailoutRange);
        }

        [SuppressMessage("ReSharper", "CompareOfFloatsByEqualityOperator")]
        public static bool IsPointInBuddhabrot(Complex c, BailoutRange bailoutRange)
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

            for (uint i = 0; i < bailoutRange.Maximum; i++)
            {
                var reTemp = re2 - im2 + c.Real;
                im = 2 * re * im + c.Imaginary;
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
                    return i >= bailoutRange.Minimum;
                }
            }

            return false;
        }
    }
}