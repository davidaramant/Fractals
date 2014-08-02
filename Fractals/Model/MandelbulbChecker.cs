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
                IsInSmallerBulb(number) ||
                IsInSmallBulbs(number);
        }

        static bool IsInLargerBulb(Complex number)
        {
            var real_minus_fourth = number.Real - 0.25;
            var q = real_minus_fourth * real_minus_fourth + number.Imag * number.Imag;

            return (q * (q + (number.Real - 0.25))) < (0.25 * number.Imag * number.Imag);
        }

        static bool IsInSmallerBulb(Complex number)
        {
            return IsInsideCircle(center: new Complex(-1, 0), radius: 0.25, number: number);
        }

        static bool IsInSmallBulbs(Complex number)
        {
            return
                IsInsideCircle(new Complex(-0.125, 0.744), 0.092, number) || 
                IsInsideCircle(new Complex(-0.125, -0.744), 0.092, number) ||
                IsInsideCircle(new Complex(-1.308, 0), 0.058, number);
        }

        static bool IsInsideCircle(Complex center, double radius, Complex number)
        {
            var translated = number - center;

            return (translated.Real * translated.Real + translated.Imag * translated.Imag) <= (radius * radius);
        }
    }
}
