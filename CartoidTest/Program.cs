using System;
using Fractals.Model;
using Fractals.Utility;
using System.Drawing;

namespace CartoidTest
{
    class Program
    {
        static void Main(string[] args)
        {
            var resolution = new Size(1000, 1000);

            var viewPoint = new Area(
                realRange: new InclusiveRange(-2, 1),
                imagRange: new InclusiveRange(-1.5, 1.5));

            var output = new Color[resolution.Width, resolution.Height];

            var borderAreas = new Area[]
            {

            };

            for (int y = 0; y < resolution.Height; y++)
            {
                for (int x = 0; x < resolution.Width; x++)
                {
                    var number = viewPoint.GetNumberFromPoint(resolution, new Point(x, y));

                    var color = PickColor(
                        isInSet: () => IsInSet(number), 
                        isInBulbs: MandelbulbChecker.IsInsideBulbs(number));

                    output[x, y] = Color.FromKnownColor(color);
                }
            }

            const double gridSize = 0.25;

            // Draw vertical lines
            for (double real = 0; real < viewPoint.RealRange.Max; real += gridSize)
            {
                var point = viewPoint.GetPointFromNumber(resolution, new Complex(real, 0));

                for (int y = 0; y < resolution.Height; y++)
                {
                    output[point.X, y] = Color.FromKnownColor(KnownColor.Green);
                }
            }
            for (double real = 0; real >= viewPoint.RealRange.Min; real -= gridSize)
            {
                var point = viewPoint.GetPointFromNumber(resolution, new Complex(real, 0));

                for (int y = 0; y < resolution.Height; y++)
                {
                    output[point.X, y] = Color.FromKnownColor(KnownColor.Green);
                }
            }

            // Draw horizontal lines
            for (double imag = 0; imag < viewPoint.ImagRange.Max; imag += gridSize)
            {
                var point = viewPoint.GetPointFromNumber(resolution, new Complex(0, imag));

                for (int x = 0; x < resolution.Width; x++)
                {
                    output[x, point.Y] = Color.FromKnownColor(KnownColor.Green);
                }
            }
            for (double imag = 0; imag >= viewPoint.ImagRange.Min; imag -= gridSize)
            {
                var point = viewPoint.GetPointFromNumber(resolution, new Complex(0, imag));

                for (int x = 0; x < resolution.Width; x++)
                {
                    output[x, point.Y] = Color.FromKnownColor(KnownColor.Green);
                }
            }

            // Draw axis
            var origin = viewPoint.GetPointFromNumber(resolution, new Complex());
            for (int x = 0; x < resolution.Width; x++)
            {
                output[x, origin.Y] = Color.FromKnownColor(KnownColor.LightGreen);
            }
            for (int y = 0; y < resolution.Height; y++)
            {
                output[origin.X, y] = Color.FromKnownColor(KnownColor.LightGreen);
            }

            var image = ImageUtility.ColorMatrixToBitmap(output);

            image.Save("output.png");
        }

        static KnownColor PickColor(Func<bool> isInSet, bool isInBulbs)
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

        static bool IsInSet(Complex c)
        {
            var z = c;

            for (int i = 0; i < 2000; i++)
            {
                z = z * z + c;

                if (z.MagnitudeSquared() > 4)
                {
                    return false;
                }
            }

            return true;
        }
        
    }
}
