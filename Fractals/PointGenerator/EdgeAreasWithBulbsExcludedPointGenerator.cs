using System;
using System.Collections.Generic;
using System.Drawing;
using Fractals.Model;
using Fractals.Utility;

namespace Fractals.PointGenerator
{
    public class EdgeAreasWithBulbsExcludedPointGenerator : BulbsExcludedPointGenerator
    {
        private Random _random;
        private List<Area> _edgeAreas;
 
        public override void Initialize(Area viewPort)
        {
            var areaCalculator = new EdgeLocator();
            _edgeAreas = areaCalculator.LocateEdges(new Size(1000, 1000), viewPort.RealRange, viewPort.ImaginaryRange);

            _random = new Random();
        }

        public override Area SelectArea(Area viewPoint)
        {
            var edgeIndex = _random.Next(_edgeAreas.Count);
            return _edgeAreas[edgeIndex];
        }
    }
}