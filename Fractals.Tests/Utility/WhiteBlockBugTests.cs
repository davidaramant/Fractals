using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fractals.Utility;
using NUnit.Framework;

namespace Fractals.Tests.Utility
{
    [TestFixture]
    public sealed class WhiteBlockBugTests
    {
        [Test]
        public void GenerateImages()
        {
            var basePath = @"C:\Users\aramant\Desktop\Broken Buddhabrot\testoutput\268";

            var names = new[] { "832", "833" };

            foreach (var name in names)
            {
                var inputPath = Path.Combine(basePath, name + ".data");
                var outputPath = Path.Combine(basePath, name + "-generated.jpg");

                var byteBuffer = File.ReadAllBytes(inputPath);

                using (var fastBitmap = new FastImage(256))
                //using (var oldBitmap = new Bitmap(256, 256, PixelFormat.Format24bppRgb))
                {
                    for (int y = 0; y < 256; y++)
                    {
                        for (int x = 0; x < 256; x++)
                        {
                            var i = y * 512 + (x * 2);

                            var current = BitConverter.ToUInt16(byteBuffer, i);
                            var hsvColor = ColorGradients.ColorCount(current);
                            var color = hsvColor.ToColor();
                            fastBitmap.SetPixel(x, y, color);
                  //          oldBitmap.SetPixel(x, y, color);
                        }
                    }
                    //for (int i = 0; i < byteBuffer.Length; i += 2)
                    //{
                    //    var current = BitConverter.ToUInt16(byteBuffer, i);
                    //    var hsvColor = ColorGradients.ColorCount(current);
                    //    var color = hsvColor.ToColor();
                    //    imageTile.SetPixel(i / 2, color);
                    //}

                    fastBitmap.Save(outputPath);
                    //oldBitmap.Save(Path.Combine(basePath, name + "-generated-old.bmp"),ImageFormat.Bmp);
                }

            }
        }
    }
}
