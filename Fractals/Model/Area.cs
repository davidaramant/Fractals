using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using Fractals.Utility;
using log4net;

namespace Fractals.Model
{
    public sealed class Area
    {
        public readonly InclusiveRange RealRange;
        public readonly InclusiveRange ImagRange;

        private readonly ILog _log;

        public Area(InclusiveRange realRange, InclusiveRange imagRange)
        {
            RealRange = realRange;
            ImagRange = imagRange;

            _log = LogManager.GetLogger(GetType());
        }

        public bool IsInside(Complex number)
        {
            return
                RealRange.IsInside(number.Real) &&
                ImagRange.IsInside(number.Imaginary);
        }

        public Point GetPointFromNumber(Size resolution, Complex number)
        {
            return new Point(
                x: (int)(resolution.Width * ((number.Real - RealRange.Minimum) / RealRange.Magnitude)),
                y: (int)(resolution.Height * ((number.Imaginary - ImagRange.Minimum) / ImagRange.Magnitude)));
        }

        public Complex GetNumberFromPoint(Size resolution, Point point)
        {
            return new Complex(
                real: RealRange.Magnitude * ((double)point.X / resolution.Width) + RealRange.Minimum,
                imaginary: ImagRange.Magnitude * (1 - ((double)point.Y / resolution.Height)) + ImagRange.Minimum);
        }

        public Complex GetRandomPoint(CryptoRandom random)
        {
            return new Complex(
                real: random.Next(RealRange),
                imaginary: random.Next(ImagRange));
        }

        public void LogViewport()
        {
            _log.DebugFormat("Viewport: Real {0}:{1}, Imaginary: {2}:{3}",
                RealRange.Minimum, RealRange.Maximum,
                ImagRange.Minimum, ImagRange.Maximum);
        }

        public IEnumerable<Complex> GetCorners()
        {
            yield return new Complex(RealRange.Minimum, ImagRange.Minimum);
            yield return new Complex(RealRange.Maximum, ImagRange.Minimum);
            yield return new Complex(RealRange.Minimum, ImagRange.Maximum);
            yield return new Complex(RealRange.Maximum, ImagRange.Maximum);
        }
    }
}
