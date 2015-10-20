using System;
using System.Collections.Generic;
using System.Drawing;

namespace Fractals.Utility
{
    public class ColorRampFactory
    {
        public static readonly ColorRamp Blue = GetIntensityRamp(196);

        public static readonly ColorRamp Psychadelic = new ColorRamp(new[]
        {
            Tuple.Create(new HsvColor(0, 1, 0), 0.0),
            Tuple.Create(new HsvColor(0, 1, 1), 0.1),
            Tuple.Create(new HsvColor(1, 1, 1), 1.0),
        });

        public static readonly ColorRamp EightiesNeonPartDeux = new ColorRamp(new[] {
                Tuple.Create(HsvColor.FromColor(Color.FromArgb(217, 150, 193)).Mutate(vx: v => 0d), 0d),
                Tuple.Create(HsvColor.FromColor(Color.FromArgb(217, 150, 193)), 0.33d),
                Tuple.Create(HsvColor.FromColor(Color.FromArgb(43, 241, 255)), 0.66d),
                Tuple.Create(HsvColor.FromColor(Color.FromArgb(43, 241, 255)).Mutate(sx: s => 0d), 1d)
            }
            );

        public static ColorRamp Rainbow
        {
            get
            {
                const int rangeCount = 18;
                const int rangeStep = 360 / rangeCount;

                var colorRanges = new List<Tuple<HsvColor, double>>();
                for (int i = 0; i <= rangeCount; i++)
                {
                    var color = new HsvColor((double) i * rangeStep / 360, 1, 1);
                    var threshold = (double)i / rangeCount;

                    colorRanges.Add(Tuple.Create(color, threshold));
                }

                return new ColorRamp(colorRanges);
            }
        }

        public static ColorRamp GetIntensityRamp(double color)
        {
            return new ColorRamp(new[]
            {
                Tuple.Create(new HsvColor(color / 360.0, 1, 0), 0.0),
                Tuple.Create(new HsvColor(color / 360.0, 1, 1), 0.97),
                Tuple.Create(new HsvColor(color / 360.0, 0, 1), 1.0),
            });
        }
    }
}
