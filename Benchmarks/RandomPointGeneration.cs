using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using Fractals.Model;
using Fractals.PointGenerator;
using Fractals.Utility;

namespace Benchmarks
{
    public class RandomPointGeneration
    {
        public int PointsToCheck = 800;

        private static readonly Lazy<Area[]> GetEdges = new Lazy<Area[]>(() =>
        {
            var edgeReader = new AreaListReader(directory: @"C:\Users\aramant\Desktop\Buddhabrot\Test Plot", filename: @"NewEdge.edge");
            return edgeReader.GetAreas().ToArray();
        });

        [Benchmark]
        public object SingleArea()
        {
            var pointGenerator = new RandomPointGenerator(AreaFactory.SearchArea, seed: 0);

            return pointGenerator.GetNumbers()
                .Where(c => !MandelbulbChecker.IsInsideBulbs(c))
                .Take(PointsToCheck)
                .Aggregate((c, sum) => c + sum);
        }

        [Benchmark]
        public object EdgeAreas()
        {
            var pointGenerator = new RandomPointGenerator(GetEdges.Value, seed: 0);

            return pointGenerator.GetNumbers()
                .Where(c => !MandelbulbChecker.IsInsideBulbs(c))
                .Take(PointsToCheck)
                .Aggregate((c, sum) => c + sum);
        }
    }
}
