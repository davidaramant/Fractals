using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        public readonly double Imag;

        public Complex()
        {
        }

        public Complex(double real, double imag)
        {
            Real = real;
            Imag = imag;
        }

        public override string ToString()
        {
            return String.Format("({0},{1}i)", Real, Imag);
        }

        /// <summary>
        /// Returns the magnitude squared.
        /// </summary>
        public double MagnitudeSquared()
        {
            return Real * Real + Imag * Imag;
        }

        public static Complex operator +(Complex c1, Complex c2)
        {
            return new Complex(c1.Real + c2.Real, c1.Imag + c2.Imag);
        }

        public static Complex operator -(Complex c1, Complex c2)
        {
            return new Complex(c1.Real - c2.Real, c1.Imag - c2.Imag);
        }

        public static Complex operator *(Complex c1, Complex c2)
        {
            return new Complex(
                        real: c1.Real * c2.Real - c1.Imag * c2.Imag,
                        imag: c1.Imag * c2.Real + c1.Real * c2.Imag
            );
        }
    }
}
