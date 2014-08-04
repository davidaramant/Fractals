using Fractals.Model;

namespace Fractals.PointGenerator
{
    public class BulbsExcludedPointGenerator : RandomPointGenerator
    {
        protected override bool ValidatePoint(Complex point)
        {
            return !MandelbulbChecker.IsInsideBulbs(point);
        }
    }
}