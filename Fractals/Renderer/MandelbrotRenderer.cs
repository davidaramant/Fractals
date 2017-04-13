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

            var startPoint = new Complex(-1, 0.25);

            var points = new[]
            {
                new Complex(-1,0.25),
                new Complex(-0.7,0.4), 
                new Complex(0.1,0.5),
                new Complex(0.9,0.55),
                new Complex(1.3,0.6),
                new Complex(1.9,0.625),
            };



            var color = PickColor(
                isCloseToPoint: () => false, //points.Any(p => IsCloseTo(number, p)),
                isInCircle: () => (number.Magnitude * number.Magnitude) < 4,
                isInEdgeRegion: () => checkForEdges && areasToInclude.Any(a => a.IsInside(number)),
                isInSet: () => MandelbrotFinder.IsInSet(number),
                isInBulbs: () => MandelbulbChecker.IsInsideBulbs(number)
            );
            return Tuple.Create(point, color);
        }

        private static bool IsCloseTo(Complex p1, Complex p2)
        {
            var diffMagnitude = (p1 - p2).Magnitude;
            return diffMagnitude * diffMagnitude < 0.001;
        }

        protected virtual IEnumerable<Area> GetAreasToInclude(Size resolution, Area viewPort)
        {
            // HACK: hardcoded file path
            var reader = new AreaListReader(@"C:\Users\aramant\Desktop\Buddhabrot\Test Plot", "Edge.edge");


            return reader.GetAreas().ToArray();
            //yield return viewPort;
        }

        protected virtual Color PickColor(
            Func<bool> isCloseToPoint,
            Func<bool> isInCircle,
            Func<bool> isInEdgeRegion,
            Func<bool> isInSet,
            Func<bool> isInBulbs)
        {
            if (isCloseToPoint())
            {
                return Color.White;
            }

            if (isInBulbs())
            {
                return Color.Black;
            }

            if (isInSet())
            {
                return Color.Aquamarine;
            }

            if (isInEdgeRegion())
            {
                return Color.DarkRed;
            }

            if (isInCircle())
            {
                return Color.Gray;
            }

            return Color.Transparent;
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
            for (double imag = 0; imag < viewPoint.ImagRange.Maximum; imag += GridSize)
            {
                Point point = viewPoint.GetPointFromNumber(resolution, new Complex(0, imag));

                for (int x = 0; x < resolution.Width; x++)
                {
                    output[x, point.Y] = Color.Green;
                }
            }
            for (double imag = 0; imag >= viewPoint.ImagRange.Minimum; imag -= GridSize)
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