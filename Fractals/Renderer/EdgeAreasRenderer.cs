using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using Fractals.Arguments;
using Fractals.Model;
using Fractals.Utility;
using log4net;

namespace Fractals.Renderer
{
    public class EdgeAreasRenderer : IGenerator
    {
        private static ILog _log;

        public EdgeAreasRenderer()
        {
            _log = LogManager.GetLogger(GetType());
        }

        public Color[,] Render(Size resolution, Area viewPort)
        {


            viewPort.LogViewport();

            // HACK: This file name should be passed in
            var areasToInclude = new AreaListReader(@"C:\Users\aramant\Desktop\Buddhabrot\Test Plot", "NewEdge.edge").GetAreas().ToArray();
            _log.Info($"Using {areasToInclude.Length} areas");

            var pixelsNeeded = (int)(viewPort.RealRange.Magnitude / areasToInclude.First().RealRange.Magnitude);

            resolution = new Size(pixelsNeeded, pixelsNeeded);
            _log.InfoFormat("Starting to render ({0:N0}x{1:N0})", resolution.Width, resolution.Height);

            var output = new Color[resolution.Width, resolution.Height];

            _log.Debug("Rendering points");

            foreach (var area in areasToInclude)
            {
                foreach (var number in area.GetCorners())
                {
                    var pixel = viewPort.GetPointFromNumber(resolution, number);
                    output[pixel.X, pixel.Y] = Color.Black;
                }
            }

            return output;
        }


    }
}