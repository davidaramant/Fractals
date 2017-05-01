using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Fractals.Arguments;
using Fractals.Model;
using log4net;

namespace Fractals.Utility
{
    public class MandelbrotFinder
    {
        private static readonly IterationRange IterationRange = new IterationRange(10 * 1000);

        private static ILog _log;

        public MandelbrotFinder()
        {
            _log = LogManager.GetLogger(GetType());
        }

        public List<Complex> FindPoints(Size resolution, Area viewPort)
        {
            _log.InfoFormat("Looking for points ({0:N0}x{1:N0})", resolution.Width, resolution.Height);

            var results = new ConcurrentBag<Complex>();

            Parallel.ForEach(resolution.GetAllPoints(), new ParallelOptions { MaxDegreeOfParallelism = GlobalArguments.DegreesOfParallelism }, point =>
            {
                var number = viewPort.GetNumberFromPoint(resolution, point);

                if (IsInSet(number))
                {
                    results.Add(number);
                }
            });

            var resultList = results.ToList();

            _log.DebugFormat("Found {0:N0} points", resultList.Count);

            return resultList;
        }

        public static bool IsInSet(Complex c)
        {
            return IsInSet(c, IterationRange);
        }

        public static bool IsInSet(Complex c, IterationRange iterationRange)
        {
            if (MandelbulbChecker.IsInsideBulbs(c))
            {
                return true;
            }

            double re = 0;
            double im = 0;

            // Check for orbits
            // - Check re/im against an old point
            // - Only check every power of 2
            double oldRe = 0;
            double oldIm = 0;

            uint checkNum = 1;

            // Cache the squares
            // They are used to find the magnitude; reuse these values when computing the next re/im
            double re2 = 0;
            double im2 = 0;

            for (uint i = 0; i < iterationRange.ExclusiveMaximum; i++)
            {
                var reTemp = re2 - im2 + c.Real;
                im = 2 * re * im + c.Imaginary;
                re = reTemp;

                // Orbit check
                if (checkNum == i)
                {
                    if (oldRe == re && oldIm == im)
                    {
                        return true;
                    }

                    oldRe = re;
                    oldIm = im;

                    checkNum = checkNum << 1;
                }

                re2 = re * re;
                im2 = im * im;

                // Check the magnitude squared against 2^2
                if ((re2 + im2) > 4)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
