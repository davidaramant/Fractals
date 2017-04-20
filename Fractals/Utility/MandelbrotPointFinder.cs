using System.Numerics;
using Fractals.Model;
using Fractals.PointGenerator;

namespace Fractals.Utility
{
    public class MandelbrotPointFinder : PointFinder
    {
        public MandelbrotPointFinder(uint minimum, uint maximum, string outputDirectory, string outputFile, IRandomPointGenerator pointGenerator)
            : base(minimum, maximum, outputDirectory, outputFile, pointGenerator)
        {
        }

        protected override bool ValidatePoint(Complex c, BailoutRange bailoutRange)
        {
            return MandelbrotFinder.IsInSet(c);
        }
    }
}