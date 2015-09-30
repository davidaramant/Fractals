using System.Numerics;

namespace Fractals.Model
{
    public static class MandelbulbChecker
    {
        /// <summary>
        /// Does a fast check to see if a complex number lies within one of the larger bulbs of the Mandelbrot set.
        /// </summary>
        public static bool IsInsideBulbs(Complex number)
        {
            return
                IsInLargerBulb(number) ||
                IsInCircularBulbs(number);
        }

        static bool IsInLargerBulb(Complex number)
        {
            var real_minus_fourth = number.Real - 0.25;
            var q = real_minus_fourth * real_minus_fourth + number.Imaginary * number.Imaginary;

            return (q * (q + (number.Real - 0.25))) < (0.25 * number.Imaginary * number.Imaginary);
        }

        static bool IsInCircularBulbs(Complex number)
        {
            return
                IsInsideCircle(new Complex(-1, 0), 0.25, number) ||
                IsInsideCircle(new Complex(-0.125, 0.744), 0.092, number) ||
                IsInsideCircle(new Complex(-0.125, -0.744), 0.092, number) ||
                IsInsideCircle(new Complex(-1.308, 0), 0.058, number);
        }

        static bool IsInsideCircle(Complex center, double radius, Complex number)
        {
            var translated = number - center;

            return (translated.Real * translated.Real + translated.Imaginary * translated.Imaginary) <= (radius * radius);
        }
    }
}
