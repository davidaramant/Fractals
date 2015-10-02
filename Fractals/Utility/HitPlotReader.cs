using System;
using System.Drawing;
using System.IO.MemoryMappedFiles;
using System.Linq;
using log4net;

namespace Fractals.Utility
{
    public sealed class HitPlotReader : IDisposable
    {
        private const int HitCountSize = sizeof(ushort);
        private const int TileResolution = 256;
        private const int TileSizeInBytes = TileResolution * TileResolution * HitCountSize;

        private readonly MemoryMappedFile _file;
        public long RowSizeInBytes { get; }
        private readonly MemoryMappedViewAccessor[] _rowAccessors;

        public HitPlotReader(string filePath, Size resolution)
        {
            var log = LogManager.GetLogger(GetType());
            log.Info($"Reading MMF: {filePath}");

            long dataSize = (long)resolution.Width * (long)resolution.Height * HitCountSize;
            var tileCount = (int)(dataSize / TileSizeInBytes);
            var tilesPerRow = resolution.Width / TileResolution;
            RowSizeInBytes = TileSizeInBytes * tilesPerRow;
            log.Info($"Size: {dataSize:N0} bytes, Tiles: {tileCount:N0}");

            _file = MemoryMappedFile.CreateFromFile(filePath);

            _rowAccessors =
                Enumerable.Range(0, resolution.Height / TileResolution).
                Select(rowIndex => _file.CreateViewAccessor(rowIndex * RowSizeInBytes, RowSizeInBytes, MemoryMappedFileAccess.Read)).
                ToArray();
        }

        public void Dispose()
        {
            foreach (var accessor in _rowAccessors)
            {
                accessor.Dispose();
            }
            _file.Dispose();
        }

        public ushort GetCount(int row, long column, long offset)
        {
            return _rowAccessors[row].ReadUInt16(column * TileSizeInBytes + offset*2);
        }
    }
}
