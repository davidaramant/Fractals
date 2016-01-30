using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fractals.Utility
{
    public sealed class HitPlotStream : IDisposable
    {
        private readonly FileStream _stream;
        private const int TileDimension = 256;
        private const int TileSizeInBytes = TileDimension * TileDimension * 2;
        private readonly Size _resolution;

        public HitPlotStream(string fileName, Size resolution)
        {
            _resolution = resolution;
            _stream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.None, bufferSize: TileSizeInBytes, useAsync: true);
        }

        public Task<byte[]> ReadTileBufferAsync()
        {
            var buffer = new byte[TileSizeInBytes];
            return _stream.ReadAsync(buffer, 0, TileSizeInBytes).ContinueWith(result => buffer);
        }

        public void SetStreamOffset(int tileNumber)
        {
            _stream.Position = (long)tileNumber * TileSizeInBytes;
        }

        public IEnumerable<ushort> GetAllCounts()
        {
            using (var reader = new BinaryReader(_stream, Encoding.UTF8, leaveOpen: true))
            {
                var max = (ulong)_resolution.Width * (ulong)_resolution.Height;
                for (ulong count = 0; count < max; count++)
                {
                    yield return reader.ReadUInt16();
                }
            }
        }

        public void Dispose()
        {
            _stream.Dispose();
        }
    }
}
