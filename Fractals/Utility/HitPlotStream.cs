using System;
using System.Collections.Generic;
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

        public HitPlotStream(string fileName)
        {
            _stream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.None, bufferSize: TileSizeInBytes, useAsync: true);
        }

        public Task<byte[]> ReadTileBufferAsync(int tileNumber)
        {
            var buffer = new byte[TileSizeInBytes];
            return _stream.ReadAsync(buffer, 0, TileSizeInBytes).ContinueWith(result => buffer);
        }

        public void Dispose()
        {
            _stream.Dispose();
        }
    }
}
