using System;
using System.Drawing;

namespace Fractals.Utility
{
    /// <summary>
    /// A color in HSV format.
    /// </summary>
    public sealed class HsvColor
    {
        public double _hue;
        public double _saturation;
        public double _value;

        public HsvColor(double hue, double saturation, double value)
        {
            _hue = hue;
            _saturation = saturation;
            _value = value;
        }

        public Color ToColor()
        {
            if (_saturation == 0)
            {
                int temp = (int)(_value * 255);

                // If s is 0, all colors are the same.
                // This is some flavor of gray.
                return Color.FromArgb(temp, temp, temp);
            }

            double r = 0;
            double g = 0;
            double b = 0;

            // Scale Hue to be between 0 and 360.
            double hueDegrees = ((double)_hue * 360) % 360;

            double p;
            double q;
            double t;

            double fractionalSector;
            int sectorNumber;
            double sectorPos;

            // The color wheel consists of 6 sectors.
            // Figure out which sector you're in.
            sectorPos = hueDegrees / 60;
            sectorNumber = (int)(Math.Floor(sectorPos));

            // get the fractional part of the sector.
            // That is, how many degrees into the sector
            // are you?
            fractionalSector = sectorPos - sectorNumber;

            // Calculate values for the three axes
            // of the color. 
            p = _value * (1 - _saturation);
            q = _value * (1 - (_saturation * fractionalSector));
            t = _value * (1 - (_saturation * (1 - fractionalSector)));

            // Assign the fractional colors to r, g, and b
            // based on the sector the angle is in.
            switch (sectorNumber)
            {
                case 0:
                    r = _value;
                    g = t;
                    b = p;
                    break;

                case 1:
                    r = q;
                    g = _value;
                    b = p;
                    break;

                case 2:
                    r = p;
                    g = _value;
                    b = t;
                    break;

                case 3:
                    r = p;
                    g = q;
                    b = _value;
                    break;

                case 4:
                    r = t;
                    g = p;
                    b = _value;
                    break;

                case 5:
                    r = _value;
                    g = p;
                    b = q;
                    break;
            }

            // Return a Color structure, with values scaled to be between 0 and 255.
            return Color.FromArgb((int)(r * 255), (int)(g * 255), (int)(b * 255));
        }
    }
}
