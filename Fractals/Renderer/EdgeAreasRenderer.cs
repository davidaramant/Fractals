using System.Collections.Generic;
using System.Drawing;
using Fractals.Model;
using Fractals.Utility;

namespace Fractals.Renderer
{
    public class EdgeAreasRenderer : MandelbrotRenderer
    {
        protected override bool ShouldIncludeGrid
        {
            get { return false; }
        }

        protected override IEnumerable<Area> GetAreasToInclude(Size resolution, InclusiveRange realAxis, InclusiveRange imaginaryAxis)
        {
            return new EdgeLocator().LocateEdges(resolution, realAxis, imaginaryAxis);
        }
    }
}