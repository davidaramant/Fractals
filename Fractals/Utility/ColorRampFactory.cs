using System;

namespace Fractals.Utility
{
    public class ColorRampFactory
    {
        public static ColorRamp Blue
        {
            get
            {
                return new ColorRamp(new[]
                {
                    Tuple.Create(new HsvColor(196.0/360.0, 1, 0), 0.0),
                    Tuple.Create(new HsvColor(196.0/360.0, 1, 1), 0.5),
                    Tuple.Create(new HsvColor(196.0/360.0, 0, 1), 1.0),
                });
            }
        }

        public static ColorRamp Psychadelic
        {
            get
            {
                return new ColorRamp(new[]
                {
                    Tuple.Create(new HsvColor(0, 1, 0), 0.0),
                    Tuple.Create(new HsvColor(0, 1, 1), 0.2),
                    Tuple.Create(new HsvColor(1, 1, 1), 1.0),
                });
            }
        }
    }
}
