using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Fractals.Model;
using Fractals.Utility;
using log4net;

namespace Fractals.Renderer
{
    public class MandelbrotRenderer : IGenerator
    {
        private static ILog _log;
        
        const double GridSize = 0.5;

        protected virtual bool ShouldIncludeGrid
        {
            get { return false; }
        }

        public MandelbrotRenderer()
        {
            _log = LogManager.GetLogger(GetType());
        }

        public Color[,] Render(Size resolution, InclusiveRange realAxis, InclusiveRange imaginaryAxis)
        {
            _log.InfoFormat("Starting to render ({0}x{1})", resolution.Width, resolution.Height);

            var viewPort = new Area(realAxis, imaginaryAxis);

            viewPort.LogViewport();

            var areasToInclude = GetAreasToInclude(resolution, realAxis, imaginaryAxis).ToArray();
            var checkForEdges = areasToInclude.Length > 1;

            var output = new Color[resolution.Width, resolution.Height];

            _log.Debug("Rendering points");
            for (int y = 0; y < resolution.Height; y++)
            {
                for (int x = 0; x < resolution.Width; x++)
                {
                    var number = viewPort.GetNumberFromPoint(resolution, new Point(x, y));
                    Color color = PickColor(
                        () => checkForEdges && areasToInclude.Any(a => a.IsInside(number)),
                        () => MandelbrotFinder.IsInSet(number),
                        () => MandelbulbChecker.IsInsideBulbs(number));
                    output[x, y] = color;
                }
            }

            if (ShouldIncludeGrid)
            {
                _log.Debug("Rendering grid");
                RenderGrid(resolution, viewPort, output);
            }

            _log.Debug("Rendering axis");
            RenderAxis(resolution, viewPort, output);

            return output;
        }

        protected virtual IEnumerable<Area> GetAreasToInclude(Size resolution, InclusiveRange realAxis, InclusiveRange imaginaryAxis)
        {
            yield return new Area(realAxis, imaginaryAxis);
        }

        protected virtual Color PickColor(Func<bool> isInEdgeRegion , Func<bool> isInSet, Func<bool> isInBulbs)
        {
            if (isInBulbs())
            {
                return Color.Gray;
            }

            if (isInSet())
            {
                return Color.Aquamarine;
            }

            if (isInEdgeRegion())
            {
                return Color.IndianRed;
            }

            return Color.Black;
        }

        private static void RenderAxis(Size resolution, Area viewPoint, Color[,] output)
        {
            // Draw axis
            Point origin = viewPoint.GetPointFromNumber(resolution, new Complex());
            for (int x = 0; x < resolution.Width; x++)
            {
                output[x, origin.Y] = Color.LightGreen;
            }
            for (int y = 0; y < resolution.Height; y++)
            {
                output[origin.X, y] = Color.LightGreen;
            }
        }

        private static void RenderGrid(Size resolution, Area viewPoint, Color[,] output)
        {
            // Draw vertical lines
            for (double real = 0; real < viewPoint.RealRange.Maximum; real += GridSize)
            {
                Point point = viewPoint.GetPointFromNumber(resolution, new Complex(real, 0));

                for (int y = 0; y < resolution.Height; y++)
                {
                    output[point.X, y] = Color.Green;
                }
            }
            for (double real = 0; real >= viewPoint.RealRange.Minimum; real -= GridSize)
            {
                Point point = viewPoint.GetPointFromNumber(resolution, new Complex(real, 0));

                for (int y = 0; y < resolution.Height; y++)
                {
                    output[point.X, y] = Color.Green;
                }
            }

            // Draw horizontal lines
            for (double imag = 0; imag < viewPoint.ImaginaryRange.Maximum; imag += GridSize)
            {
                Point point = viewPoint.GetPointFromNumber(resolution, new Complex(0, imag));

                for (int x = 0; x < resolution.Width; x++)
                {
                    output[x, point.Y] = Color.Green;
                }
            }
            for (double imag = 0; imag >= viewPoint.ImaginaryRange.Minimum; imag -= GridSize)
            {
                Point point = viewPoint.GetPointFromNumber(resolution, new Complex(0, imag));

                for (int x = 0; x < resolution.Width; x++)
                {
                    output[x, point.Y] = Color.Green;
                }
            }
        }
    }
}