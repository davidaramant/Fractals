using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Fractals.Arguments;
using Fractals.Model;
using log4net;

namespace Fractals.Utility
{
    public class EdgeLocator
    {
        private readonly static ILog _log;

        static EdgeLocator()
        {
            _log = LogManager.GetLogger(typeof (EdgeLocator));
        }

        public static List<Area> LocateEdges(Size resolution, double gridSize, Area viewPort)
        {
            _log.DebugFormat("Looking for intersting areas ({0}x{1})", resolution.Width, resolution.Height);

            var allAreas = AllPossibleAreas(viewPort.RealRange, viewPort.ImaginaryRange, gridSize);
            _log.DebugFormat("Found {0} total areas", allAreas.Count);

            var numbers = new MandelbrotFinder().FindPoints(resolution, viewPort);
            _log.DebugFormat("Found {0} points within the region", numbers.Count);

            var areasWithSomeNumbers = FindAreasWithNumbers(allAreas, numbers);
            _log.InfoFormat("Found {0} areas bordering points", areasWithSomeNumbers.Count);

            return areasWithSomeNumbers;
        }

        private static List<Area> FindAreasWithNumbers(IEnumerable<Area> allAreas, IEnumerable<Complex> numbers)
        {
            var results = new ConcurrentBag<Area>();

            var processedCount = 0;
            Parallel.ForEach(numbers, new ParallelOptions { MaxDegreeOfParallelism = GlobalArguments.DegreesOfParallelism }, number =>
            {
                var containingAreas = allAreas.Where(a => a.IsInside(number));
                foreach (var containingArea in containingAreas)
                {
                    results.Add(containingArea);
                }

                Interlocked.Increment(ref processedCount);
                if (processedCount % 10000 == 0)
                {
                    _log.DebugFormat("Checked {0} points", processedCount);
                }
            });
            _log.DebugFormat("Checked {0} points", processedCount);

            return results.Distinct().ToList();
        }

        private static List<Area> AllPossibleAreas(InclusiveRange realAxis, InclusiveRange imaginaryAxis, double gridSize)
        {
            var allAreas = new List<Area>();

            var realPoints = new List<double>();
            for (var realPoint = realAxis.Minimum; realPoint <= realAxis.Maximum; realPoint += gridSize)
            {
                realPoints.Add(realPoint);
            }

            var imaginaryPoints = new List<double>();
            for (var imaginaryPoint = imaginaryAxis.Minimum; imaginaryPoint <= imaginaryAxis.Maximum; imaginaryPoint += gridSize)
            {
                imaginaryPoints.Add(imaginaryPoint);
            }

            for (var realIndex = 0; realIndex < realPoints.Count - 1; realIndex++)
            {
                for (var imaginaryIndex = 0; imaginaryIndex < imaginaryPoints.Count - 1; imaginaryIndex++)
                {
                    var realRange = new InclusiveRange(realPoints[realIndex], realPoints[realIndex + 1]);
                    var imaginaryRange = new InclusiveRange(imaginaryPoints[imaginaryIndex], imaginaryPoints[imaginaryIndex + 1]);

                    var gridBox = new Area(
                        realRange,
                        imaginaryRange);
                    allAreas.Add(gridBox);
                }
            }

            return allAreas;
        }
    }
}