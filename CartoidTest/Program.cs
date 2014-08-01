using Fractals.Model;
using Fractals.Utility;
using System.Drawing;

namespace CartoidTest
{
    class Program
    {
        static void Main(string[] args)
        {
            var resolution = new Size(500, 500);

            var viewPoint = new Viewport(
                realRange: new InclusiveRange(-3, 1),
                imagRange: new InclusiveRange(-2, 2));

            var output = new Color[resolution.Width, resolution.Height];

            for (int y = 0; y < resolution.Height; y++)
            {
                for (int x = 0; x < resolution.Width; x++)
                {
                    var number = viewPoint.GetNumberFromPoint(resolution, new Point(x, y));

                    var color = PickColor(
                        isInSet: IsInSet(number), 
                        isInBulbs: MandelbulbChecker.IsInsideBulbs(number), 
                        isOnAxis: number.Real == 0 || number.Imag == 0);

                    output[x, y] = Color.FromKnownColor(color);
                }
            }

            var image = ImageUtility.ColorMatrixToBitmap(output);

            image.Save("output.png");
        }

        static KnownColor PickColor(bool isInSet, bool isInBulbs, bool isOnAxis)
        {
            if (isInBulbs)
            {
                return KnownColor.Gray;
            }

            if (isInSet)
            {
                return KnownColor.Aquamarine;
            }

            if (isOnAxis)
            {
                return KnownColor.White;
            }

            return KnownColor.Black;
        }

        static bool IsInSet(Complex c)
        {
            var z = c;

            for (int i = 0; i < 1000; i++)
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
