using System.Numerics;
using Fractals.Model;
using Fractals.PointGenerator;

namespace Fractals.Utility
{
    public class MandelbrotPointFinder : PointFinder
    {
        public MandelbrotPointFinder(int minimum, int maximum, string outputDirectory, string outputFile, IRandomPointGenerator pointGenerator)
            : base(minimum, maximum, outputDirectory, outputFile, pointGenerator)
        {
        }

        protected override bool ValidatePoint(Complex c, IterationRange iterationRange)
        {
            return MandelbrotFinder.IsInSet(c);
        }
    }
}