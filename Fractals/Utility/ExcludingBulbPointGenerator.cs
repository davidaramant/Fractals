using Fractals.Model;

namespace Fractals.Utility
{
    public class ExcludingBulbPointGenerator : RandomPointGenerator
    {
        protected override bool ValidatePoint(Complex point)
        {
            return !MandelbulbChecker.IsInsideBulbs(point);
        }
    }
}