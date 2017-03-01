using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using Fractals.Utility;

namespace Benchmarks
{
    public class BitmapVsFastImage
    {
        private static readonly Size Resolution = new Size(256, 256);

        private string _path = Path.GetTempFileName();

        public const int JpgQuality = 85;
        private static readonly ImageCodecInfo JgpEncoder = GetEncoder(ImageFormat.Jpeg);
        private static readonly EncoderParameters QualitySetting = CreateQualityParameter();

        private static ImageCodecInfo GetEncoder(ImageFormat format)
        {
            return ImageCodecInfo.GetImageDecoders().FirstOrDefault(codec => codec.FormatID == format.Guid);
        }

        private static EncoderParameters CreateQualityParameter()
        {
            // Create an EncoderParameters object.
            // An EncoderParameters object has an array of EncoderParameter
            // objects. In this case, there is only one
            // EncoderParameter object in the array.
            var encoderParams = new EncoderParameters(1);
            var encoderParam = new EncoderParameter(Encoder.Quality, (long)JpgQuality);
            encoderParams.Param[0] = encoderParam;

            return encoderParams;
        }


        [Benchmark(Baseline = true)]
        public void SystemImagingBitmap()
        {
            using (var bmp = new Bitmap(Resolution.Width, Resolution.Height))
            {
                for (int y = 0; y < Resolution.Height; y++)
                {
                    for (int x = 0; x < Resolution.Width; x++)
                    {
                        bmp.SetPixel(x, y, Color.Red);
                    }
                }

                bmp.Save(_path, JgpEncoder, QualitySetting);
            }
            File.Delete(_path);
        }

        [Benchmark]
        public void FastImage()
        {
            using (var img = new FastImage(Resolution.Width, Resolution.Height))
            {
                for (int y = 0; y < Resolution.Height; y++)
                {
                    for (int x = 0; x < Resolution.Width; x++)
                    {
                        img.SetPixel(x, y, Color.Red);
                    }
                }

                img.Save(_path);
            }
            File.Delete(_path);
        }
    }
}
