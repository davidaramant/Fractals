using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace Fractals.Utility
{
    // HACK: This class sometimes produces corrupt PNGs when using PixelFormat.Format24bppRgb.  Is this a framework bug?
    public sealed class FastBitmap : IDisposable
    {
        const PixelFormat Format = PixelFormat.Format32bppArgb;
        readonly int _pixelSizeInBytes = Image.GetPixelFormatSize(Format) / 8;
        readonly int _stride;
        readonly byte[] _pixelBuffer;

        public int Width { get; }
        public int Height { get; }

        public FastBitmap(int tileSize) : this(tileSize, tileSize)
        {
        }

        public FastBitmap(Size resolution) : this(resolution.Width, resolution.Height)
        {
        }

        public FastBitmap(int width, int height)
        {
            Width = width;
            Height = height;
            _stride = width * _pixelSizeInBytes;
            _pixelBuffer = new byte[_stride * height];
        }

        public void SetPixel(int x, int y, HsvColor hsvColor)
        {
            SetPixel(x, y, hsvColor.ToColor());
        }

        public void SetPixel(int pixelIndex, HsvColor hsvColor)
        {
            SetPixel(pixelIndex, hsvColor.ToColor());
        }

        public void SetPixel(int x, int y, Color color)
        {
            var index = y * _stride + x * _pixelSizeInBytes;
            SetPixelFromIndex(index, color);
        }

        public void SetPixel(int pixelIndex, Color color)
        {
            var index = pixelIndex * _pixelSizeInBytes;
            SetPixelFromIndex(index, color);
        }

        private void SetPixelFromIndex(int index, Color color)
        {
            _pixelBuffer[index] = color.B;
            _pixelBuffer[index + 1] = color.G;
            _pixelBuffer[index + 2] = color.R;
            _pixelBuffer[index + 3] = Byte.MaxValue;
        }

        public void Save(string filePath)
        {
            using (var bmp = new Bitmap(Width, Height, Format))
            {
                var bmpData = bmp.LockBits(
                    new Rectangle(0, 0, Width, Height), 
                    ImageLockMode.WriteOnly, 
                    bmp.PixelFormat);

                // Get the address of the first line.
                IntPtr ptr = bmpData.Scan0;

                // Copy the RGB values back to the bitmap
                Marshal.Copy(_pixelBuffer, 0, ptr, _pixelBuffer.Length);

                // Unlock the bits.
                bmp.UnlockBits(bmpData);
                bmp.Save(filePath);
            }
        }

        public void Dispose()
        {
        }
    }
}
