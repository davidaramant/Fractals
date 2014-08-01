using System;
using System.Drawing;
using Fractals.Model;

namespace Fractals.Renderer
{
    public class MandelbrotRenderer
    {
        public Color[,] Render()
        {
            var resolution = new Size(1000, 1000);

            var viewPoint = new Area(new InclusiveRange(-2, 1), new InclusiveRange(-1.5, 1.5));

            var output = new Color[resolution.Width, resolution.Height];

            for (int y = 0; y < resolution.Height; y++)
            {
                for (int x = 0; x < resolution.Width; x++)
                {
                    Complex number = viewPoint.GetNumberFromPoint(resolution, new Point(x, y));

                    KnownColor color = PickColor(() => IsInSet(number), MandelbulbChecker.IsInsideBulbs(number));

                    output[x, y] = Color.FromKnownColor(color);
                }
            }

            const double gridSize = 0.25;

            // Draw vertical lines
            for (double real = 0; real < viewPoint.RealRange.Max; real += gridSize)
            {
                Point point = viewPoint.GetPointFromNumber(resolution, new Complex(real, 0));

                for (int y = 0; y < resolution.Height; y++)
                {
                    output[point.X, y] = Color.FromKnownColor(KnownColor.Green);
                }
            }
            for (double real = 0; real >= viewPoint.RealRange.Min; real -= gridSize)
            {
                Point point = viewPoint.GetPointFromNumber(resolution, new Complex(real, 0));

                for (int y = 0; y < resolution.Height; y++)
                {
                    output[point.X, y] = Color.FromKnownColor(KnownColor.Green);
                }
            }

            // Draw horizontal lines
            for (double imag = 0; imag < viewPoint.ImagRange.Max; imag += gridSize)
            {
                Point point = viewPoint.GetPointFromNumber(resolution, new Complex(0, imag));

                for (int x = 0; x < resolution.Width; x++)
                {
                    output[x, point.Y] = Color.FromKnownColor(KnownColor.Green);
                }
            }
            for (double imag = 0; imag >= viewPoint.ImagRange.Min; imag -= gridSize)
            {
                Point point = viewPoint.GetPointFromNumber(resolution, new Complex(0, imag));

                for (int x = 0; x < resolution.Width; x++)
                {
                    output[x, point.Y] = Color.FromKnownColor(KnownColor.Green);
                }
            }

            // Draw axis
            Point origin = viewPoint.GetPointFromNumber(resolution, new Complex());
            for (int x = 0; x < resolution.Width; x++)
            {
                output[x, origin.Y] = Color.FromKnownColor(KnownColor.LightGreen);
            }
            for (int y = 0; y < resolution.Height; y++)
            {
                output[origin.X, y] = Color.FromKnownColor(KnownColor.LightGreen);
            }

            return output;
        }

        private static KnownColor PickColor(Func<bool> isInSet, bool isInBulbs)
        {
            if (isInBulbs)
            {
                return KnownColor.Gray;
            }

            if (isInSet())
            {
                return KnownColor.Aquamarine;
            }

            return KnownColor.Black;
        }

        private static bool IsInSet(Complex c)
        {
            Complex z = c;

            for (int i = 0; i < 2000; i++)
            {
                z = z*z + c;

                if (z.MagnitudeSquared() > 4)
                {
                    return false;
                }
            }

            return true;
        }
    }
}