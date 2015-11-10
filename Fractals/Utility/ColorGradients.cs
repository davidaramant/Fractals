using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Fractals.Utility
{
    public class ColorGradients
    {
        public static readonly ColorGradient Blue = GetIntensityGradient(196);

        public static readonly ColorGradient Psychadelic = new ColorGradient(new[]
        {
            Tuple.Create(new HsvColor(0, 1, 0), 0.0),
            Tuple.Create(new HsvColor(0, 1, 1), 0.1),
            Tuple.Create(new HsvColor(1, 1, 1), 1.0),
        });

        public static readonly ColorGradient EightiesNeonPartDeux = new ColorGradient(ExpandRange(new[] {
                Tuple.Create(HsvColor.FromColor(Color.FromArgb(217, 150, 193)), 0.10d),
                Tuple.Create(HsvColor.FromColor(Color.FromArgb(43, 241, 255)), 0.20d),
            })
            );

        public static readonly ColorGradient Neon = new ColorGradient(ExpandRange(new[] {
                Tuple.Create(HsvColor.FromColor(Color.FromArgb(255, 24, 24)), 0.10d),
                Tuple.Create(HsvColor.FromColor(Color.FromArgb(255, 150, 88)), 0.15d),
                Tuple.Create(HsvColor.FromColor(Color.FromArgb(67, 227, 39)), 0.19d),
                Tuple.Create(HsvColor.FromColor(Color.FromArgb(102, 237, 255)), 0.23d),
                Tuple.Create(HsvColor.FromColor(Color.FromArgb(163,142,255)), 0.29d),
                Tuple.Create(HsvColor.FromColor(Color.FromArgb(239, 66, 255)), 0.40d),
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

        public static ColorGradient Rainbow
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

                return new ColorGradient(colorRanges);
            }
        }

        public static ColorGradient GetIntensityGradient(double color)
        {
            return new ColorGradient(new[]
            {
                Tuple.Create(new HsvColor(color / 360.0, 1, 0), 0.0),
                Tuple.Create(new HsvColor(color / 360.0, 1, 1), 0.97),
                Tuple.Create(new HsvColor(color / 360.0, 0, 1), 1.0),
            });
        }
    }
}
