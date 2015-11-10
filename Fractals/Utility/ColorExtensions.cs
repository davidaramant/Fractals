using System.Drawing;

namespace Fractals.Utility
{
    public static class ColorExtensions
    {
        public static HsvColor ToHsv(this Color color)
        {
            return HsvColor.FromColor(color);
        }

        public static double GetLuminance( this Color color )
        {
            return 0.2126*color.R + 0.7152*color.G + 0.0722*color.B;
        }
    }
}
