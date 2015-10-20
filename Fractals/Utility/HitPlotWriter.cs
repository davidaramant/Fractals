using System;
using System.Drawing;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;
using log4net;

namespace Fractals.Utility
{
    public sealed class HitPlotWriter : IDisposable
    {
        private const int HitCountSize = sizeof(ushort);
        private const int TileResolution = 256;
        private const int TileSizeInBytes = TileResolution * TileResolution * HitCountSize;

        private readonly MemoryMappedFile _file;
        private readonly Size _resolution;
        private readonly int _tileCount;
        private readonly object[] _accessorLocks;
        private readonly MemoryMappedViewAccessor[] _accessors;

        public HitPlotWriter(string filePath, Size resolution)
        {
            var log = LogManager.GetLogger(GetType());
            _resolution = resolution;
            long dataSize = (long)_resolution.Width * (long)_resolution.Height * (long)HitCountSize;
            _tileCount = (int)(dataSize / TileSizeInBytes);
            log.Info($"Size: {dataSize:N0} bytes, Tiles: {_tileCount:N0}");

            if (!File.Exists(filePath))
            {
                log.Info("Creating MMF: " + filePath);
                using (var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    fs.SetLength(dataSize);
                }
            }
            else
            {
                log.Info("Using existing file: " + filePath);
            }
            _file = MemoryMappedFile.CreateFromFile(filePath);

            _accessorLocks = Enumerable.Range(0, _tileCount).Select(_ => new object()).ToArray();
            _accessors =
                Enumerable.Range(0, _tileCount).
                Select(i => (long)i * (long)TileSizeInBytes).
                Select(offset => _file.CreateViewAccessor(offset, TileSizeInBytes)).
                ToArray();
        }

        public void Dispose()
        {
            for (int i = 0; i < _tileCount; i++)
            {
                _accessors[i].Dispose();
            }

            _file.Dispose();
        }

        public void IncrementPoint(Point p)
        {
            var tileIndex = PointToTileIndex(p);
            var positionInTile = PointToTilePosition(p);

            lock (_accessorLocks[tileIndex])
            {
                var currentCount = _accessors[tileIndex].ReadUInt16(positionInTile);
                if (currentCount < ushort.MaxValue)
                {
                    currentCount++;
                    _accessors[tileIndex].Write(positionInTile, currentCount);
                }
            }
        }

        private int PointToTileIndex(Point p)
        {
            return (p.X / TileResolution) + (p.Y / TileResolution) * (_resolution.Width / TileResolution);
        }

        private int PointToTilePosition(Point p)
        {
            return ((p.X % TileResolution) + (p.Y % TileResolution) * TileResolution) * HitCountSize;
        }
    }
}