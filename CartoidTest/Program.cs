using System.Drawing;
using Fractals.Model;
using Fractals.Renderer;
using Fractals.Utility;

namespace CartoidTest
{
    internal class Program
    {
        private static void Main()
        {
            var resolution = new Size(1000, 1000);
            var realAxis = new InclusiveRange(-2, 1);
            var imaginaryAxis = new InclusiveRange(-1.5, 1.5);

            Color[,] output = new InterestingPointsRenderer().Render(resolution, realAxis, imaginaryAxis);

            Bitmap image = ImageUtility.ColorMatrixToBitmap(output);

            image.Save("output.png");
        }
    }
}