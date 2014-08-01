using System.Drawing;

namespace Fractals.Utility
{
    public static class ImageUtility
    {
        public static Bitmap ColorMatrixToBitmap(Color[,] colors)
        {
            var width = colors.GetLength(0);
            var height = colors.GetLength(1);

            var img = new Bitmap(width, height);

            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    img.SetPixel(i, j, colors[i,j]);
                }
            }

            return img;
        }
    }
}
