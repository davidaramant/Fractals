using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Text;
using log4net;

namespace Fractals.Utility
{
    public sealed class HitPlotReader : IDisposable
    {
        private const int HitCountSize = sizeof(ushort);
        private const int TileResolution = 256;
        private const int TileSizeInBytes = TileResolution * TileResolution * HitCountSize;

        private readonly MemoryMappedFile _file;
        private readonly int _tilesPerRow;
        private readonly long _rowSizeInBytes;
        private readonly MemoryMappedViewStream[] _rowAccessors;

        public HitPlotReader(string filePath, Size resolution)
        {
            var log = LogManager.GetLogger(GetType());
            log.Info($"Reading MMF: {filePath}");

            long dataSize = (long)resolution.Width * (long)resolution.Height * HitCountSize;
            var tileCount = (int)(dataSize / TileSizeInBytes);
            _tilesPerRow = resolution.Width / TileResolution;
            _rowSizeInBytes = TileSizeInBytes * _tilesPerRow;
            log.Info($"Size: {dataSize:N0} bytes, Tiles: {tileCount:N0}");

            _file = MemoryMappedFile.CreateFromFile(filePath);

            _rowAccessors =
                Enumerable.Range(0, resolution.Height / TileResolution).
                Select(rowIndex => _file.CreateViewStream(rowIndex * _rowSizeInBytes, _rowSizeInBytes, MemoryMappedFileAccess.Read)).
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

        public RowReader2 ReadRow(int rowNumber)
        {
            //            return new RowReader(_rowAccessors[rowNumber], _tilesPerRow, _rowSizeInBytes, rowNumber);
            return new RowReader2(_rowAccessors[rowNumber], _tilesPerRow);
        }


        public sealed class RowReader2 : IDisposable
        {
            private readonly MemoryMappedViewStream _stream;
            private readonly BinaryReader _reader;
            private readonly int _tilesPerRow;

            internal RowReader2(MemoryMappedViewStream stream, int tilesPerRow)
            {
                _stream = stream;
                _reader = new BinaryReader(_stream, Encoding.UTF8, leaveOpen: true);
                _tilesPerRow = tilesPerRow;
            }

            //public IEnumerable<ushort[]> GetTiles()
            //{
            //    var tileBuffer = new ushort[TileResolution * TileResolution];

            //    for (int tileIndex = 0; tileIndex < _tilesPerRow; tileIndex++)
            //    {
            //        for (int i = 0; i < tileBuffer.Length; i++)
            //        {
            //            tileBuffer[i] = _reader.ReadUInt16();
            //        }
            //        yield return tileBuffer;
            //    }
            //}

            public void FillBufferWithTile(ushort[] buffer)
            {
                for (int i = 0; i < buffer.Length; i++)
                {
                    buffer[i] = _reader.ReadUInt16();
                }
            }

            public void Dispose()
            {
                _reader.Dispose();
            }
        }


        public sealed class RowReader : IDisposable
        {
            private readonly MemoryMappedViewStream _stream;
            private readonly BinaryReader _reader;
            private readonly int _tilesPerRow;

            internal RowReader(MemoryMappedFile file, int tilesPerRow, long rowSizeInBytes, int row)
            {
                _stream = file.CreateViewStream(rowSizeInBytes * row, rowSizeInBytes, MemoryMappedFileAccess.Read);
                _reader = new BinaryReader(_stream);
                _tilesPerRow = tilesPerRow;
            }

            public IEnumerable<ushort[]> GetTiles()
            {
                var tileBuffer = new ushort[TileResolution * TileResolution];

                for (int tileIndex = 0; tileIndex < _tilesPerRow; tileIndex++)
                {
                    for (int i = 0; i < tileBuffer.Length; i++)
                    {
                        tileBuffer[i] = _reader.ReadUInt16();
                    }
                    yield return tileBuffer;
                }
            }

            public void Dispose()
            {
                _reader.Dispose();
                _stream.Dispose();
            }
        }
    }
}
