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
        public readonly double Re;

        /// <summary>
        /// The imaginary component of the number.
        /// </summary>
        public readonly double Im;

        public Complex()
        {
        }

        public Complex(double real, double imaginary)
        {
            Re = real;
            Im = imaginary;
        }

        public override string ToString()
        {
            return String.Format("({0},{1}i)", Re, Im);
        }

        public static Complex operator +(Complex c1, Complex c2)
        {
            return new Complex(c1.Re + c2.Re, c1.Im + c2.Im);
        }

        public static Complex operator -(Complex c1, Complex c2)
        {
            return new Complex(c1.Re - c2.Re, c1.Im - c2.Im);
        }

        public static Complex operator *(Complex c1, Complex c2)
        {
            return new Complex(
                        real:c1.Re * c2.Re - c1.Im * c2.Im,
                        imaginary:c1.Im * c2.Re + c1.Re * c2.Im
            );
        }
    }
}
