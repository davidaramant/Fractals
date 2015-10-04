using System;
using System.Drawing;

namespace Fractals.Utility
{
    /// <summary>
    /// A color in HSV format.
    /// </summary>
    public sealed class HsvColor
    {
        public double Hue { get; }
        public double Saturation { get; }
        public double Value { get; }

        public HsvColor(double hue, double saturation, double value)
        {
            Hue = hue;
            Saturation = saturation;
            Value = value;
        }

        public Color ToColor()
        {
            if (Saturation == 0)
            {
                int temp = (int)(Value * 255);

                // If s is 0, all colors are the same.
                // This is some flavor of gray.
                return Color.FromArgb(temp, temp, temp);
            }

            double r = 0;
            double g = 0;
            double b = 0;

            // Scale Hue to be between 0 and 360.
            double hueDegrees = ((double)Hue * 360) % 360;

            // The color wheel consists of 6 sectors.
            // Figure out which sector you're in.
            var sectorPos = hueDegrees / 60;
            var sectorNumber = (int)(Math.Floor(sectorPos));

            // get the fractional part of the sector.
            // That is, how many degrees into the sector
            // are you?
            var fractionalSector = sectorPos - sectorNumber;

            // Calculate values for the three axes
            // of the color. 
            var p = Value * (1 - Saturation);
            var q = Value * (1 - (Saturation * fractionalSector));
            var t = Value * (1 - (Saturation * (1 - fractionalSector)));

            // Assign the fractional colors to r, g, and b
            // based on the sector the angle is in.
            switch (sectorNumber)
            {
                case 0:
                    r = Value;
                    g = t;
                    b = p;
                    break;

                case 1:
                    r = q;
                    g = Value;
                    b = p;
                    break;

                case 2:
                    r = p;
                    g = Value;
                    b = t;
                    break;

                case 3:
                    r = p;
                    g = q;
                    b = Value;
                    break;

                case 4:
                    r = t;
                    g = p;
                    b = Value;
                    break;

                case 5:
                    r = Value;
                    g = p;
                    b = q;
                    break;
            }

            // Return a Color structure, with values scaled to be between 0 and 255.
            return Color.FromArgb((int)(r * 255), (int)(g * 255), (int)(b * 255));
        }
    }
}
