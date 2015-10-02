using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace Fractals.Utility
{
    public sealed class ImageTile : IDisposable
    {
        const PixelFormat Format = PixelFormat.Format24bppRgb;
        readonly int _pixelFormatSize = Image.GetPixelFormatSize(Format) / 8;
        readonly int _stride;
        readonly byte[] _pixelBuffer;
        readonly GCHandle _handle;
        private readonly Bitmap _tile;

        public ImageTile(int tileSize)
        {
            _stride = tileSize * _pixelFormatSize;
            _pixelBuffer = new byte[_stride * tileSize];
            _handle = GCHandle.Alloc(_pixelBuffer, GCHandleType.Pinned);
            IntPtr pointer = Marshal.UnsafeAddrOfPinnedArrayElement(_pixelBuffer, 0);
            _tile = new Bitmap(tileSize, tileSize, _stride, Format, pointer);
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
            _tile.Save(filePath);
        }

        public void Dispose()
        {
            _tile.Dispose();
            _handle.Free();
        }
    }
}
