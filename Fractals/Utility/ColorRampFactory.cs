using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

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

        public static readonly ColorRamp EightiesNeonPartDeux = new ColorRamp(ExpandRange(new[] {
                Tuple.Create(HsvColor.FromColor(Color.FromArgb(217, 150, 193)), 0.10d),
                Tuple.Create(HsvColor.FromColor(Color.FromArgb(43, 241, 255)), 0.20d),
            })
            );

        public static readonly ColorRamp Neon = new ColorRamp(ExpandRange(new[] {
                Tuple.Create(HsvColor.FromColor(Color.FromArgb(255, 0, 0)), 0.10d),
                Tuple.Create(HsvColor.FromColor(Color.FromArgb(255, 255, 0)), 0.20d),
                Tuple.Create(HsvColor.FromColor(Color.FromArgb(0, 255, 0)), 0.30d),
                Tuple.Create(HsvColor.FromColor(Color.FromArgb(0, 255, 255)), 0.40d),
                Tuple.Create(HsvColor.FromColor(Color.FromArgb(122, 142, 255)), 0.50d),
                Tuple.Create(HsvColor.FromColor(Color.FromArgb(255, 0, 255)), 0.60d),
            })
        );


        private static IEnumerable<Tuple<HsvColor, double>> ExpandRange(Tuple<HsvColor, double>[] colors)
        {
            yield return Tuple.Create(colors.First().Item1.Mutate(vx: v => 0d), 0d);
            foreach (var c in colors)
            {
                yield return c;
            }
            yield return Tuple.Create(colors.Last().Item1.Mutate(sx: s => 0d), 1d);
        }

        public static ColorRamp Rainbow
        {
            get
            {
                const int rangeCount = 18;
                const int rangeStep = 360 / rangeCount;

                var colorRanges = new List<Tuple<HsvColor, double>>();
                for (int i = 0; i <= rangeCount; i++)
                {
                    var color = new HsvColor((double)i * rangeStep / 360, 1, 1);
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
