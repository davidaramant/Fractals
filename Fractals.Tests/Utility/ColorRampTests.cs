using System;
using System.Collections.Generic;
using System.Drawing;
using Fractals.Utility;
using NUnit.Framework;

namespace Fractals.Tests.Utility
{
    [TestFixture]
    public sealed class ColorRampTests
    {
        [Test]
        public void ShouldRampUpBetweenTwoColors()
        {
            MakeStrip(new[]
                {
                    Tuple.Create(new HsvColor(0, 0, 0), 0.0),
                    Tuple.Create(new HsvColor(0, 0, 1), 1.0)
                },
                @"C:\temp\out\simpleramp.png");
        }

        [Test]
        public void ShouldWrapHueAcrossMaximum()
        {
            MakeStrip(new[]
                {
                    Tuple.Create(new HsvColor(0.9, 1, 1), 0.0),
                    Tuple.Create(new HsvColor(0.1, 1, 1), 1.0)
                },
                @"C:\temp\out\huerolloverramp.png");
        }


        [Test]
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
                @"C:\temp\out\rangeramp.png");
        }

        private static void MakeStrip(IEnumerable<Tuple<HsvColor, double>> colorPoints, string filePath)
        {
            var ramp = new ColorRamp(colorPoints);

            using (var image = new FastBitmap(500, 50))
            {
                for (int x = 0; x < image.Width; x++)
                {
                    var color = ramp.GetColor((double)x / image.Width).ToColor();

                    for (int y = 0; y < image.Height; y++)
                    {
                        image.SetPixel(x, y, color);
                    }
                }

                image.Save(filePath);
            }
        }
    }
}
