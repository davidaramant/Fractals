using System.Collections.Generic;
using System.Drawing;
using Fractals.Model;

namespace Fractals.Utility
{
    public class MandelbrotFinder
    {
        public static IEnumerable<Complex> FindPoints(Size resolution, InclusiveRange realAxis, InclusiveRange imaginaryAxis)
        {
            var results = new List<Complex>();
            var viewPoint = new Area(realAxis, imaginaryAxis);

            for (int y = 0; y < resolution.Height; y++)
            {
                for (int x = 0; x < resolution.Width; x++)
                {
                    var number = viewPoint.GetNumberFromPoint(resolution, new Point(x, y));

                    if (IsInSet(number))
                    {
                        results.Add(number);
                    }
                }
            }

            return results;
        }

        public static bool IsInSet(Complex c)
        {
            Complex z = c;

            for (int i = 0; i < 2000; i++)
            {
                z = z * z + c;

                if (z.MagnitudeSquared() > 4)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
