using System.Collections.Generic;
using System.Numerics;

namespace Fractals.PointGenerator
{
    public interface IRandomPointGenerator
    {
        IEnumerable<Complex> GetNumbers();
    }
}