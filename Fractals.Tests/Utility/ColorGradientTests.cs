using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using Fractals.Utility;
using NUnit.Framework;

namespace Fractals.Tests.Utility
{
    [TestFixture]
    public sealed class ColorGradientTests
    {
        private static readonly string Outdir =
            Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                //"Test Plot",
                "Color Ramps");

        [SetUp]
        public void CreateOutputDirectory()
        {
            Directory.CreateDirectory(Outdir);
        }

        [Test]
        //[Ignore("Manual")]
        public void RgbVsHsvGradient()
        {
            var start = Color.DodgerBlue;
            var end = Color.YellowGreen;

            MakeRgbStrip(start, end, "rgbramp.png");

            MakeStrip(
                new[]
                {
                    Tuple.Create(start.ToHsv(),0.0),
                    Tuple.Create(end.ToHsv(),1.0)
                },
                "hsvramp.png");
        }

        [Test]
        [Ignore("Manual")]
        public void ShouldRampUpBetweenTwoColors()
        {
            MakeStrip(new[]
            {
                Tuple.Create(new HsvColor(0, 0, 0), 0.0),
                Tuple.Create(new HsvColor(0, 0, 1), 1.0)
            },
                "simpleramp.png");
        }

        [Test]
        [Ignore("Manual")]
        public void ShouldWrapHueAcrossMaximum()
        {
            MakeStrip(new[]
            {
                Tuple.Create(new HsvColor(0.9, 1, 1), 0.0),
                Tuple.Create(new HsvColor(0.1, 1, 1), 1.0)
            },
                "huerolloverramp.png");
        }


        [Test]
        [Ignore("Manual")]
        public void ShouldDoMultiPointRamp()
        {
            MakeStrip(new[]
            {
                Tuple.Create(new HsvColor(213.0/360.0, 0.82, 0), 0.0),
                Tuple.Create(new HsvColor(213.0/360.0, 0.82, 1), 0.1),
                Tuple.Create(new HsvColor(196.0/360.0, 1, 1), 0.15),
                Tuple.Create(new HsvColor(196.0/360.0, 0.1, 1), 0.3),
                Tuple.Create(new HsvColor(196.0/360.0, 0, 1), 1.0),

            },
                "rangeramp.png");
        }

        [Test]
        [Ignore("Manual")]
        public void GiantGoldfishPalette()
        {
            MakeStrip(new[]
            {
                Tuple.Create(HsvColor.FromColor(Color.FromArgb(105,210,231)).Mutate(vx:_=>0d),0d),
                Tuple.Create(HsvColor.FromColor(Color.FromArgb(105,210,231)),0.20d),
                Tuple.Create(HsvColor.FromColor(Color.FromArgb(167,219,216)),0.40d),
                Tuple.Create(HsvColor.FromColor(Color.FromArgb(224,228,204)),0.60d),
                Tuple.Create(HsvColor.FromColor(Color.FromArgb(243,134,48)),0.80d),
                Tuple.Create(HsvColor.FromColor(Color.FromArgb(250,105,0)),1d),
            },
            "Giant Goldfish.png");
        }

        [Test]
        [Ignore("Manual")]
        public void EightiesNeonPalette()
        {
            MakeStrip(new[]
            {
                Tuple.Create(HsvColor.FromColor(Color.FromArgb(154,47,255)).Mutate(vx:_=>0d),0d),
                Tuple.Create(HsvColor.FromColor(Color.FromArgb(154,47,255)),0.20d),
                Tuple.Create(HsvColor.FromColor(Color.FromArgb(252,55,239)),0.40d),
                Tuple.Create(HsvColor.FromColor(Color.FromArgb(0,153,255)),0.60d),
                Tuple.Create(HsvColor.FromColor(Color.FromArgb(34,255,104)),0.80d),
                Tuple.Create(HsvColor.FromColor(Color.FromArgb(255,242,49)),1d),
            },
            "80s Neon.png");
        }

        [Test]
        [Ignore("Manual")]
        public void NeonMonsterPalette()
        {
            MakeStrip(new[]
            {
                Tuple.Create(HsvColor.FromColor(Color.FromArgb(176,235,233)).Mutate(vx:v=>0d),0d),
                Tuple.Create(HsvColor.FromColor(Color.FromArgb(176,235,233)),0.17d),
                Tuple.Create(HsvColor.FromColor(Color.FromArgb(177,223,246)),0.34d),
                Tuple.Create(HsvColor.FromColor(Color.FromArgb(200,185,218)),0.50d),
                Tuple.Create(HsvColor.FromColor(Color.FromArgb(217,150,193)),0.68d),
                Tuple.Create(HsvColor.FromColor(Color.FromArgb(243,108,167)),0.85d),
                Tuple.Create(HsvColor.FromColor(Color.FromArgb(243,108,167)).Mutate(sx:s=>0d),1d),
            },
            "Neon Monster.png");
        }

        [Test]
        [Ignore("Manual")]
        public void EightiesNeonPartDeuxPalette()
        {
            MakeStrip(ColorGradients.EightiesNeonPartDeux,
            "80s Neon Part Deux.png");
        }

        [Test]
        [Ignore("Manual")]
        public void NeonPalette()
        {
            PrintGradientInfo(ColorGradients.Neon);
            MakeStrip(ColorGradients.Neon,
                "Neon.png");
        }

        [Test]
        [Ignore("Manual")]
        public void RainbowPalette()
        {
            MakeStrip(ColorGradients.Rainbow,
            "Rainbow.png");
        }

        [Test]
        [Ignore("Manual")]
        public void FirePalette()
        {
            PrintGradientInfo(ColorGradients.Fire);
            MakeStrip(ColorGradients.Fire,
                "Fire.png");
        }

        [Test]
        [Ignore("Manual")]
        public void PastelPalette()
        {
            PrintGradientInfo(ColorGradients.Pastel);
            MakeStrip(ColorGradients.Pastel,
                "Pastel.png");
        }

        [Test]
        [Ignore("Manual")]
        public void BluePalette()
        {
            PrintGradientInfo(ColorGradients.Blue);
            MakeStrip(ColorGradients.Blue,
                "Blue.png");
        }

        [Test]
        [Ignore("Manual")]
        public void ManualRainbowPalette()
        {
            PrintGradientInfo(ColorGradients.ManualRainbow);
            MakeStrip(ColorGradients.ManualRainbow,
                "ManualRainbow.png");
        }

        [Test]
        [Ignore("Manual")]
        public void ManualRainbow2Palette()
        {
            PrintGradientInfo(ColorGradients.ManualRainbow2);
            MakeStrip(ColorGradients.ManualRainbow2,
                "ManualRainbow2.png");
        }

        private static void PrintGradientInfo(ColorGradient gradient)
        {
            string GetInfo(HsvColor hsvColor, double value) => $"{value:000%} : {hsvColor} : Luminance: {hsvColor.ToColor().GetLuminance()}";

            foreach (var colorRange in gradient)
            {
                Console.WriteLine(GetInfo(colorRange.StartColor, colorRange.Start));
            }
            Console.WriteLine(GetInfo(gradient.Last().EndColor, gradient.Last().End));
        }

        private void MakeStrip(IEnumerable<Tuple<HsvColor, double>> colorPoints, string fileName)
        {
            var ramp = new ColorGradient(colorPoints);

            MakeStrip(ramp, fileName);
        }

        private void MakeRgbStrip(Color start, Color end, string fileName)
        {
            using (var image = new FastImage(500, 50))
            {
                for (int x = 0; x < image.Width; x++)
                {
                    var color = Lerp(start, end, (double)x / image.Width);

                    for (int y = 0; y < image.Height; y++)
                    {
                        image.SetPixel(x, y, color);
                    }
                }

                image.Save(Path.Combine(Outdir, fileName));
            }
        }

        private static Color Lerp(Color start, Color end, double ratio)
        {
            return Color.FromArgb(
                red: Interpolate(start.R, end.R, ratio),
                green: Interpolate(start.G, end.G, ratio),
                blue: Interpolate(start.B, end.B, ratio));
        }

        private static int Interpolate(byte start, byte end, double ratio)
        {
            return (int)(255.0 * Interpolate(start / 255.0, end / 255.0, ratio));
        }

        private static double Interpolate(double v0, double v1, double ratio)
        {
            return v0 + ratio * (v1 - v0);
        }

        private void MakeStrip(ColorGradient gradient, string fileName)
        {
            var evenGradient = new ColorGradient(
                gradient.Select((range, index) => Tuple.Create(range.StartColor, (double)index / gradient.Count())).
                Concat(new[] { Tuple.Create(gradient.Last().EndColor, 1d) })
                );

            using (var image = new FastImage(500, 50))
            {
                for (int x = 0; x < image.Width; x++)
                {
                    var color = evenGradient.GetColor((double)x / image.Width).ToColor();

                    for (int y = 0; y < image.Height; y++)
                    {
                        image.SetPixel(x, y, color);
                    }
                }

                image.Save(Path.Combine(Outdir, fileName));
            }
        }
    }
}
