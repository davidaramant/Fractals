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
                Tuple.Create(Color.FromArgb(217, 150, 193).ToHsv(), 0.10d),
                Tuple.Create(Color.FromArgb(43, 241, 255).ToHsv(), 0.20d),
            })
            );

        public static readonly ColorGradient Neon = new ColorGradient(ExpandRange(new[] {
                Tuple.Create(Color.FromArgb(255, 24, 24).ToHsv(), 0.10),
                Tuple.Create(Color.FromArgb(255, 150, 88).ToHsv(), 0.20),
                Tuple.Create(Color.FromArgb(151, 234, 110).ToHsv(), 0.30),
                Tuple.Create(Color.FromArgb(0, 255, 255).ToHsv(), 0.40),
            })
        );

        public static readonly ColorGradient Fire = new ColorGradient(ExpandRange(new[] {
                Tuple.Create(Color.FromArgb(255, 40, 40).ToHsv(), 0.16),
                Tuple.Create(Color.FromArgb(230,230,0).ToHsv(), 0.30),
            })
        );

        public static readonly ColorGradient Pastel = new ColorGradient(ExpandRange(new[]
        {
            Tuple.Create(Color.FromArgb(255,140,149).ToHsv(),0.10),
            Tuple.Create(Color.FromArgb(255,170,168).ToHsv(),0.15),
            Tuple.Create(Color.FromArgb(252,212,182).ToHsv(),0.20),
            Tuple.Create(Color.FromArgb(221,235,193).ToHsv(),0.25),
            Tuple.Create(Color.FromArgb(168,230,207).ToHsv(),0.30),
            Tuple.Create(Color.FromArgb(175,225,240).ToHsv(),0.35),
            Tuple.Create(Color.FromArgb(239,186,237).ToHsv(),0.40),
        }));

        public static readonly ColorGradient ManualRainbow = new ColorGradient(ExpandRange(new[]{
            Tuple.Create(Color.FromArgb(66,132,255).ToHsv(),0.12520950544396),
            Tuple.Create(Color.FromArgb(87,202,204).ToHsv(),0.22949248029599),
            Tuple.Create(Color.FromArgb(146,255,132).ToHsv(),0.333217849157742),
        }));

        // Large plot values:
        // 15, 33, 55   4000

        // Small plot values
        // 34, 81, 136  2000

        public static readonly ColorGradient ManualRainbow2 = new ColorGradient(new[]{
            Tuple.Create(Color.FromArgb(66,132,255).ToHsv().Mutate(vx:v=>0.1),0d),
            Tuple.Create(Color.FromArgb(66,132,255).ToHsv(),15.0/CappedMax),
            Tuple.Create(Color.FromArgb(87,202,204).ToHsv(),33.0/CappedMax),
            Tuple.Create(Color.FromArgb(167,239,200).ToHsv(),55.0/CappedMax),
            Tuple.Create(Color.FromArgb(167,239,200).ToHsv().Mutate(sx:s=>0,vx:v=>1),1d),
        });

        const ushort CappedMax = 500;

        public static HsvColor ColorCount(ushort currentCount)
        {
            if (currentCount == 0) return HsvColor.Black;
            currentCount = Math.Min(currentCount, CappedMax);
            //var ratio = Gamma(1.0 - Math.Pow(Math.E, -15.0 * currentCount / CappedMax));
            //return ManualRainbow2.GetColor(ratio);
            return ManualRainbow2.GetColor((double)currentCount / CappedMax);
        }

        private static double Gamma(double x, double exp = 1.2)
        {
            return Math.Pow(x, 1.0 / exp);
        }


        private static IEnumerable<Tuple<HsvColor, double>> ExpandRange(Tuple<HsvColor, double>[] colors)
        {
            yield return Tuple.Create(colors.First().Item1.Mutate(vx: v => 0d), 0d);
            foreach (var c in colors)
            {
                yield return c;
            }
            yield return Tuple.Create(colors.Last().Item1.Mutate(sx: s => 0d, vx: v => 1d), 1d);
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
                Tuple.Create(new HsvColor(color / 360.0, 1, 1), 0.5),
                Tuple.Create(new HsvColor(color / 360.0, 0, 1), 1.0),
            });
        }
    }
}
