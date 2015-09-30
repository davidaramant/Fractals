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

        public Color[,] Render(Size resolution, Area viewPort)
        {
            _log.InfoFormat("Starting to render ({0:N0}x{1:N0})", resolution.Width, resolution.Height);

            viewPort.LogViewport();

            var areasToInclude = GetAreasToInclude(resolution, viewPort).ToArray();
            var checkForEdges = areasToInclude.Length > 1;

            var output = new Color[resolution.Width, resolution.Height];
            
            _log.Debug("Rendering points");

            var processedPixels = resolution
                .GetAllPoints()
                .AsParallel()
                .WithDegreeOfParallelism(GlobalArguments.DegreesOfParallelism)
                .Select(p => PickColorForPoint(p, viewPort, resolution, checkForEdges, areasToInclude))
                .AsEnumerable();

            foreach (var result in processedPixels)
            {
                output[result.Item1.X, result.Item1.Y] = result.Item2;
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

        private Tuple<Point, Color> PickColorForPoint(Point point, Area viewPort, Size resolution, bool checkForEdges, IEnumerable<Area> areasToInclude)
        {
            var number = viewPort.GetNumberFromPoint(resolution, point);
            var color = PickColor(
                () => checkForEdges && areasToInclude.Any(a => a.IsInside(number)),
                () => MandelbrotFinder.IsInSet(number),
                () => MandelbulbChecker.IsInsideBulbs(number));
            return Tuple.Create(point, color);
        }

        protected virtual IEnumerable<Area> GetAreasToInclude(Size resolution, Area viewPort)
        {
            yield return viewPort;
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