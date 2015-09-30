using System.Numerics;
using Fractals.Model;

namespace Fractals.PointGenerator
{
    public class BulbsOnlyPointGenerator : RandomPointGenerator
    {
        protected override bool ValidatePoint(Complex point)
        {
            return MandelbulbChecker.IsInsideBulbs(point);
        }
    }
}