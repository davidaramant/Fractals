using System.Drawing;
using Fractals.Utility;

namespace Fractals.Model
{
    public sealed class Area
    {
        public readonly InclusiveRange RealRange;
        public readonly InclusiveRange ImagRange;

        public Area(InclusiveRange realRange, InclusiveRange imagRange)
        {
            RealRange = realRange;
            ImagRange = imagRange;
        }

        public bool IsInside(Complex number)
        {
            return
                RealRange.IsInside(number.Real) &&
                ImagRange.IsInside(number.Imag);
        }

        public Point GetPointFromNumber(Size resolution, Complex number)
        {
            return new Point(
                x: (int)(resolution.Width * ((number.Real - RealRange.Min) / RealRange.Magnitude)),
                y: (int)(resolution.Height * ((number.Imag - ImagRange.Min) / ImagRange.Magnitude)));
        }

        public Complex GetNumberFromPoint(Size resolution, Point point)
        {
            return new Complex(
                real: RealRange.Magnitude * ((double)point.X / resolution.Width) + RealRange.Min,
                imag: ImagRange.Magnitude * ((double)point.Y / resolution.Height) + ImagRange.Min);
        }

        public Complex GetRandomPoint(CryptoRandom random)
        {
            return new Complex(
                real: random.Next(RealRange), 
                imag: random.Next(ImagRange));
        }
    }
}
