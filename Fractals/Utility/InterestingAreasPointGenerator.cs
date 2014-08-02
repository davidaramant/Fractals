using System;
using System.Collections.Generic;
using System.Drawing;
using Fractals.Model;

namespace Fractals.Utility
{
    public class InterestingAreasPointGenerator : ExcludingBulbPointGenerator
    {
        private Random _random;
        private List<Area> _interestingAreas;
 
        public override void Initialize(Area viewPort)
        {
            var areaCalculator = new AreaCalculator();
            _interestingAreas = areaCalculator.InterestingAreas(new Size(1000, 1000), viewPort.RealRange, viewPort.ImagRange);

            _random = new Random();
        }

        public override Area SelectArea(Area viewPoint)
        {
            var areaIndex = _random.Next(_interestingAreas.Count);
            return _interestingAreas[areaIndex];
        }
    }
}