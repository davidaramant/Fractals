using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace Fractals.Utility
{
    // TODO: This class may be spitting out broken PNGs that ImageMagick chokes on
    public sealed class FastBitmap : IDisposable
    {
        const PixelFormat Format = PixelFormat.Format24bppRgb;
        readonly int _pixelFormatSize = Image.GetPixelFormatSize(Format) / 8;
        readonly int _stride;
        readonly byte[] _pixelBuffer;
        readonly GCHandle _handle;
        private readonly Bitmap _image;

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
            _stride = width * _pixelFormatSize;
            _pixelBuffer = new byte[_stride * height];
            _handle = GCHandle.Alloc(_pixelBuffer, GCHandleType.Pinned);
            IntPtr pointer = Marshal.UnsafeAddrOfPinnedArrayElement(_pixelBuffer, 0);
            _image = new Bitmap(width, height, _stride, Format, pointer);
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
            var index = y * _stride + x * _pixelFormatSize;
            SetPixelFromIndex(index, color);
        }

        public void SetPixel(int pixelIndex, Color color)
        {
            var index = pixelIndex * _pixelFormatSize;
            SetPixelFromIndex(index, color);
        }

        private void SetPixelFromIndex(int index, Color color)
        {
            _pixelBuffer[index] = color.B;
            _pixelBuffer[index + 1] = color.G;
            _pixelBuffer[index + 2] = color.R;
        }

        public void Save(string filePath)
        {
            _image.Save(filePath);
        }

        public void Dispose()
        {
            _image.Dispose();
            _handle.Free();
        }
    }
}
