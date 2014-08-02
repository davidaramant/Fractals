using System.Collections.Generic;
using Fractals.Model;

namespace Fractals.Utility
{
    public class RandomPointGenerator
    {
        public IEnumerable<Complex> GetRandomComplexNumbers(Area viewPort)
        {
            var rand = new CryptoRandom();
            while (true)
            {
                yield return GetPossiblePoint(rand, SelectArea(viewPort));
            }
        }

        public virtual void Initialize(Area viewPort)
        {
        }

        public virtual Area SelectArea(Area viewPoint)
        {
            return viewPoint;
        }

        protected Complex GetPossiblePoint(CryptoRandom random, Area viewPort)
        {
            Complex point;
            do
            {
                var area = SelectArea(viewPort);
                point = area.GetRandomPoint(random);
            } while (!ValidatePoint(point));

            return point;
        }

        protected virtual bool ValidatePoint(Complex point)
        {
            return true;
        }
    }
}