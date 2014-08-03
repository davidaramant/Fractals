using System.Collections.Generic;
using System.Drawing;

namespace Fractals.Utility
{
    public static class SystemDrawingExtensions
    {
        public static Point Rotate(this Point p)
        {
            return new Point(p.Y, p.X);
        }

        public static bool IsInside(this Size size, Point p)
        {
            return
                p.X >= 0 && p.X < size.Width &&
                p.Y >= 0 && p.Y < size.Height;
        }


        public static IEnumerable<Point> GetAllPoints(this Size resolution)
        {
            for (int x = 0; x < resolution.Width; x++)
            {
                for (int y = 0; y < resolution.Height; y++)
                {
                    yield return new Point(x, y);
                }
            }
        }
    }
}
