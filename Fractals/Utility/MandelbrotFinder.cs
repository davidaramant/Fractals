using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using Fractals.Model;
using log4net;

namespace Fractals.Utility
{
    public class MandelbrotFinder
    {
        private const int Bailout = 10000;

        private static ILog _log;

        public MandelbrotFinder()
        {
            _log = LogManager.GetLogger(GetType());
        }

        public List<Complex> FindPoints(Size resolution, InclusiveRange realAxis, InclusiveRange imaginaryAxis)
        {
            _log.InfoFormat("Looking for points ({0}x{1})", resolution.Width, resolution.Height);

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

            var resultList = results.ToList();

            _log.DebugFormat("Found {0} points", resultList.Count);

            return resultList;
        }

        public static bool IsInSet(Complex c)
        {
            var rePrev = c.Real;
            var imPrev = c.Imaginary;

            double re = 0;
            double im = 0;

            for (int i = 0; i < Bailout; i++)
            {
                var reTemp = re * re - im * im + rePrev;
                im = 2 * re * im + imPrev;
                re = reTemp;

                var magnitudeSquared = re * re + im * im;
                if (magnitudeSquared > 4)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
