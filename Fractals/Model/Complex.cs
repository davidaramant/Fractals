using System;
using System.Diagnostics;

namespace Fractals.Model
{
    [DebuggerDisplay("{ToString()}")]
    public sealed class Complex
    {
        /// <summary>
        /// The real component of the number.
        /// </summary>
        public readonly double Real;

        /// <summary>
        /// The imaginary component of the number.
        /// </summary>
        public readonly double Imaginary;

        public Complex()
        {
        }

        public Complex(double real, double imaginary)
        {
            Real = real;
            Imaginary = imaginary;
        }

        public override string ToString()
        {
            return String.Format("({0},{1}i)", Real, Imaginary);
        }

        /// <summary>
        /// Returns the magnitude squared.
        /// </summary>
        public double MagnitudeSquared()
        {
            return Real * Real + Imaginary * Imaginary;
        }

        public static Complex operator +(Complex c1, Complex c2)
        {
            return new Complex(c1.Real + c2.Real, c1.Imaginary + c2.Imaginary);
        }

        public static Complex operator -(Complex c1, Complex c2)
        {
            return new Complex(c1.Real - c2.Real, c1.Imaginary - c2.Imaginary);
        }

        public static Complex operator *(Complex c1, Complex c2)
        {
            return new Complex(
                        real: c1.Real * c2.Real - c1.Imaginary * c2.Imaginary,
                        imaginary: c1.Imaginary * c2.Real + c1.Real * c2.Imaginary
            );
        }
    }
}
