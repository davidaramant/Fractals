using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using Fractals.Model;

namespace Fractals.Utility
{
    public class AreaCalculator
    {
        const double GridSize = 0.05;
        
        public IEnumerable<Area> InterestingAreas(Size resolution, InclusiveRange realAxis, InclusiveRange imaginaryAxis)
        {
            var allAreas = AllPossibleAreas(realAxis, imaginaryAxis);
            var numbers = MandelbrotFinder.FindPoints(resolution, realAxis, imaginaryAxis);
            var areasWithSomeNumbers = FindAreasWithNumbers(allAreas, numbers);

            return areasWithSomeNumbers;
        }

        private IEnumerable<Area> FindAreasWithNumbers(IEnumerable<Area> allAreas, IEnumerable<Complex> numbers)
        {
            var results = new ConcurrentBag<Area>();

            Parallel.ForEach(numbers, number =>
            {
                var containingAreas = allAreas.Where(a => a.IsInside(number));
                foreach (var containingArea in containingAreas)
                {
                    results.Add(containingArea);
                }
            });

            return results.Distinct();
        }

        private IEnumerable<Area> AllPossibleAreas(InclusiveRange realAxis, InclusiveRange imaginaryAxis)
        {
            var allAreas = new List<Area>();

            var realPoints = new List<double>();
            for (var realPoint = realAxis.Min; realPoint <= realAxis.Max; realPoint += GridSize)
            {
                realPoints.Add(realPoint);
            }

            var imaginaryPoints = new List<double>();
            for (var imaginaryPoint = imaginaryAxis.Min; imaginaryPoint <= imaginaryAxis.Max; imaginaryPoint += GridSize)
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