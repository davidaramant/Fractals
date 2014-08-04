using System;
using System.Collections.Generic;
using System.Drawing;
using Fractals.Model;
using Fractals.Utility;

namespace Fractals.PointGenerator
{
    public class EdgeAreasWithBulbsExcludedPointGenerator : BulbsExcludedPointGenerator
    {
        private const double GridSize = 0.04;

        private Random _random;
        private List<Area> _edgeAreas;
 
        public override void Initialize(Area viewPort)
        {
            _edgeAreas = EdgeLocator.LocateEdges(new Size(1000, 1000), GridSize, viewPort);

            _random = new Random();
        }

        public override Area SelectArea(Area viewPoint)
        {
            var edgeIndex = _random.Next(_edgeAreas.Count);
            return _edgeAreas[edgeIndex];
        }
    }
}