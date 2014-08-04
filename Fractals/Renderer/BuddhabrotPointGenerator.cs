using System;
using Fractals.Model;

namespace Fractals.Renderer
{
    public static class BuddhabrotPointGenerator
    {
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
                    if (IsPracticallyTheSame(oldRe, re) && IsPracticallyTheSame(oldIm, im))
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

        private static bool IsPracticallyTheSame(double v1, double v2)
        {
            return Math.Abs(v1 - v2) <= 1e-17;
        }
    }
}