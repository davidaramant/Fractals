using System;
using System.Drawing;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;

namespace Fractals.Utility
{
    public sealed class MemoryMappedHitPlot : IDisposable
    {
        private readonly MemoryMappedFile _file;

        private const int SegmentCount = 400;
        private readonly long _segmentSizeInBytes;

        private readonly object[] _accessorLocks;
        private readonly MemoryMappedViewAccessor[] _accessors;

        private const long PlotSizeOffset = 4 * sizeof(int);
        private const long HitCountSize = sizeof(ushort);

        private ushort _max = 0;
        private readonly bool _openedForWrite;

        public Size Resolution { get; private set; }

        public static MemoryMappedHitPlot OpenForSaving(string filePath, Size resolution)
        {
            return new MemoryMappedHitPlot(filePath, resolution);
        }

        private MemoryMappedHitPlot(string filePath, Size resolution)
        {
            _openedForWrite = true;
            Resolution = resolution;

            Console.Out.WriteLine("Creating MMF: " + filePath);
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
            long dataSize = (long)Resolution.Width * (long)Resolution.Height * HitCountSize;
            long fullSize = PlotSizeOffset + dataSize;
            Console.Out.WriteLine("Size: " + fullSize);
            using (var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                fs.SetLength(fullSize);
            }
            _file = MemoryMappedFile.CreateFromFile(filePath);

            using (var plotSizeAccessor = _file.CreateViewAccessor(0, 2 * sizeof(int)))
            {
                plotSizeAccessor.Write(0, Resolution.Width);
                plotSizeAccessor.Write(sizeof(int), Resolution.Height);
            }

            _accessorLocks = Enumerable.Range(0, SegmentCount).Select(_ => new object()).ToArray();
            _segmentSizeInBytes = dataSize / SegmentCount;
            _accessors =
                Enumerable.Range(0, SegmentCount).
                Select(i => PlotSizeOffset + i * _segmentSizeInBytes).
                Select(offset => _file.CreateViewAccessor(offset, _segmentSizeInBytes)).
                ToArray();
        }

        public static MemoryMappedHitPlot OpenForReading(string filePath)
        {
            return new MemoryMappedHitPlot(filePath);
        }

        private MemoryMappedHitPlot(string filePath)
        {
            Console.Out.WriteLine("Reading MMF: " + filePath);
            _file = MemoryMappedFile.CreateFromFile(filePath);
            using (var plotSizeAccessor = _file.CreateViewAccessor(0, PlotSizeOffset))
            {
                var width = plotSizeAccessor.ReadInt32(0);
                var height = plotSizeAccessor.ReadInt32(sizeof(int));
                _max = plotSizeAccessor.ReadUInt16(2 * sizeof(int));

                Resolution = new Size(width, height);
            }

            long dataSize = (long)Resolution.Width * (long)Resolution.Height * HitCountSize;

            _accessorLocks = Enumerable.Range(0, SegmentCount).Select(_ => new object()).ToArray();
            _segmentSizeInBytes = dataSize / SegmentCount;
            _accessors =
                Enumerable.Range(0, SegmentCount).
                Select(i => PlotSizeOffset + i * _segmentSizeInBytes).
                Select(offset => _file.CreateViewAccessor(offset, _segmentSizeInBytes)).
                ToArray();
        }

        public void Dispose()
        {
            for (int i = 0; i < SegmentCount; i++)
            {
                _accessors[i].Dispose();
            }

            if (_openedForWrite)
            {
                using (var maxAccessor = _file.CreateViewAccessor(2 * sizeof(int), sizeof(ushort)))
                {
                    maxAccessor.Write(0, _max);
                }
            }

            _file.Dispose();
        }

        public void IncrementPoint(Point p)
        {
            var position = PointToPosition(p);
            var segmentIndex = PositionToSegmentIndex(position);
            var segmentPosition = position % _segmentSizeInBytes;

            lock (_accessorLocks[segmentIndex])
            {
                var currentCount = _accessors[segmentIndex].ReadUInt16(segmentPosition);
                currentCount++;
                _accessors[segmentIndex].Write(segmentPosition, currentCount);
            }
        }

        public ushort GetHitsForPoint(Point p)
        {
            var position = PointToPosition(p);
            var segmentIndex = PositionToSegmentIndex(position);
            var segmentPosition = position % _segmentSizeInBytes;

            return _accessors[segmentIndex].ReadUInt16(segmentPosition);
        }

        private long PointToPosition(Point p)
        {
            return (long)PlotSizeOffset + (long)Resolution.Width * HitCountSize * (long)p.Y + HitCountSize * (long)p.X;
        }

        private long PositionToSegmentIndex(long position)
        {
            return (position - PlotSizeOffset) / _segmentSizeInBytes;
        }

        public uint GetMax()
        {
            if (_max == 0)
            {
                const int bufferCount = 100;
                long bufferSizeInBytes = _segmentSizeInBytes / bufferCount;
                long numberOfElementsInBuffer = bufferSizeInBytes/HitCountSize;

                _max = ParallelEnumerable.Range(0, SegmentCount).
                   Select(segmentIndex =>
                   {
                       var maxes = new ushort[bufferCount];
                       var buffer = new ushort[numberOfElementsInBuffer];

                       for (int bufferIndex = 0; bufferIndex < bufferCount; bufferIndex++)
                       {
                           _accessors[segmentIndex].ReadArray(
                               position: bufferIndex * bufferSizeInBytes,
                               array: buffer,
                               offset: 0,
                               count: (int)numberOfElementsInBuffer);

                           maxes[bufferIndex] = buffer.Max();
                       }

                       return maxes.Max();
                   }).
                   Max();
            }

            return _max;
        }
    }
}