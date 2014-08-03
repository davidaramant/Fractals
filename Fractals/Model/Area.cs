using System.Drawing;
using Fractals.Utility;
using log4net;

namespace Fractals.Model
{
    public sealed class Area
    {
        public readonly InclusiveRange RealRange;
        public readonly InclusiveRange ImaginaryRange;

        private readonly ILog _log;

        public Area(InclusiveRange realRange, InclusiveRange imaginaryRange)
        {
            RealRange = realRange;
            ImaginaryRange = imaginaryRange;

            _log = LogManager.GetLogger(GetType());
        }

        public bool IsInside(Complex number)
        {
            return
                RealRange.IsInside(number.Real) &&
                ImaginaryRange.IsInside(number.Imaginary);
        }

        public Point GetPointFromNumber(Size resolution, Complex number)
        {
            return new Point(
                x: (int)(resolution.Width * ((number.Real - RealRange.Minimum) / RealRange.Magnitude)),
                y: (int)(resolution.Height * ((number.Imaginary - ImaginaryRange.Minimum) / ImaginaryRange.Magnitude)));
        }

        public Complex GetNumberFromPoint(Size resolution, Point point)
        {
            return new Complex(
                real: RealRange.Magnitude * ((double)point.X / resolution.Width) + RealRange.Minimum,
                imaginary: ImaginaryRange.Magnitude * ((double)point.Y / resolution.Height) + ImaginaryRange.Minimum);
        }

        public Complex GetRandomPoint(CryptoRandom random)
        {
            return new Complex(
                real: random.Next(RealRange),
                imaginary: random.Next(ImaginaryRange));
        }

        public void LogViewport()
        {
            _log.DebugFormat("Viewport: Real {0}:{1}, Imaginary: {2}:{3}",
                RealRange.Minimum, RealRange.Maximum,
                ImaginaryRange.Minimum, ImaginaryRange.Maximum);
        }
    }
}
