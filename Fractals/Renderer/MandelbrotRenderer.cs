using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Fractals.Model;
using Fractals.Utility;

namespace Fractals.Renderer
{
    public class MandelbrotRenderer
    {
        public Color[,] Render(Size resolution, InclusiveRange realAxis, InclusiveRange imaginaryAxis)
        {
            var viewPoint = new Area(realAxis, imaginaryAxis);
            var areasToInclude = GetAreasToInclude(resolution, realAxis, imaginaryAxis).ToArray();

            var output = new Color[resolution.Width, resolution.Height];

            for (int y = 0; y < resolution.Height; y++)
            {
                for (int x = 0; x < resolution.Width; x++)
                {
                    var number = viewPoint.GetNumberFromPoint(resolution, new Point(x, y));
                    Color color = PickColor(() => areasToInclude.Any(a => a.IsInside(number)), () => MandelbrotFinder.IsInSet(number), () => MandelbulbChecker.IsInsideBulbs(number));
                    output[x, y] = color;
                }
            }

            RenderGrid(resolution, viewPoint, output);
            RenderAxis(resolution, viewPoint, output);

            return output;
        }

        protected virtual IEnumerable<Area> GetAreasToInclude(Size resolution, InclusiveRange realAxis, InclusiveRange imaginaryAxis)
        {
            yield return new Area(realAxis, imaginaryAxis);
        }

        protected virtual Color PickColor(Func<bool> isInArea , Func<bool> isInSet, Func<bool> isInBulbs)
        {
            if (!isInArea())
            {
                return Color.IndianRed;
            }

            if (isInBulbs())
            {
                return Color.Gray;
            }

            if (isInSet())
            {
                return Color.Aquamarine;
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
            const double gridSize = 0.25;

            // Draw vertical lines
            for (double real = 0; real < viewPoint.RealRange.Max; real += gridSize)
            {
                Point point = viewPoint.GetPointFromNumber(resolution, new Complex(real, 0));

                for (int y = 0; y < resolution.Height; y++)
                {
                    output[point.X, y] = Color.Green;
                }
            }
            for (double real = 0; real >= viewPoint.RealRange.Min; real -= gridSize)
            {
                Point point = viewPoint.GetPointFromNumber(resolution, new Complex(real, 0));

                for (int y = 0; y < resolution.Height; y++)
                {
                    output[point.X, y] = Color.Green;
                }
            }

            // Draw horizontal lines
            for (double imag = 0; imag < viewPoint.ImagRange.Max; imag += gridSize)
            {
                Point point = viewPoint.GetPointFromNumber(resolution, new Complex(0, imag));

                for (int x = 0; x < resolution.Width; x++)
                {
                    output[x, point.Y] = Color.Green;
                }
            }
            for (double imag = 0; imag >= viewPoint.ImagRange.Min; imag -= gridSize)
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