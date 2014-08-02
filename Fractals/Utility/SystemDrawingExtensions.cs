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
    }
}
