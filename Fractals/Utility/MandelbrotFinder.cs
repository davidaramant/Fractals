using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using Fractals.Model;

namespace Fractals.Utility
{
    public class MandelbrotFinder
    {
        public static IEnumerable<Complex> FindPoints(Size resolution, InclusiveRange realAxis, InclusiveRange imaginaryAxis)
        {
            var results = new ConcurrentBag<Complex>();
            var viewPoint = new Area(realAxis, imaginaryAxis);

            Parallel.For(0, resolution.Height, y =>
            {
                for (int x = 0; x < resolution.Width; x++)
                {
                    var number = viewPoint.GetNumberFromPoint(resolution, new Point(x, y));

                    if (IsInSet(number))
                    {
                        results.Add(number);
                    }
                }
            });

            return results;
        }

        public static bool IsInSet(Complex c)
        {
            Complex z = c;

            for (int i = 0; i < 5000; i++)
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
