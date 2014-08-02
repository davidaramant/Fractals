using System;
using System.Collections.Generic;
using System.Drawing;
using Fractals.Model;
using Fractals.Utility;

namespace Fractals.Renderer
{
    public class InterestingPointsRenderer : MandelbrotRenderer
    {
        protected override bool ShouldIncludeGrid
        {
            get { return false; }
        }

        protected override IEnumerable<Area> GetAreasToInclude(Size resolution, InclusiveRange realAxis, InclusiveRange imaginaryAxis)
        {
            return new AreaCalculator().InterestingAreas(resolution, realAxis, imaginaryAxis);
        }
    }
}