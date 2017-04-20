using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Fractals.Model;

namespace Fractals.PointGenerator
{
    public sealed class RandomPointGenerator : IRandomPointGenerator
    {
        private readonly Random _random;
        private readonly Area[] _edgeAreas;

        public RandomPointGenerator(Area viewPort, int? seed = null) : 
            this(new[] { viewPort }, seed)
        {
        }

        public RandomPointGenerator(IEnumerable<Area> edgeAreas, int? seed = null)
        {
            _random = seed.HasValue ? new Random(seed.Value) : new Random();
            _edgeAreas = edgeAreas.ToArray();
        }

        public IEnumerable<Complex> GetNumbers()
        {
            while (true)
            {
                var edgeIndex = _random.Next(_edgeAreas.Length);
                yield return _edgeAreas[edgeIndex].GetRandomPoint(_random);
            }
        }
    }
}
