using System.Drawing;
using Fractals.Renderer;
using Fractals.Utility;

namespace CartoidTest
{
    internal class Program
    {
        private static void Main()
        {
            Color[,] output = new MandelbrotRenderer().Render();
            Bitmap image = ImageUtility.ColorMatrixToBitmap(output);

            image.Save("output.png");
        }
    }
}