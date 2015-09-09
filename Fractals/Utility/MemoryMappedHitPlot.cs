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

        private const int SegmentCount = 10;
        private readonly long SegmentSize;

        private readonly object[] _accessorLocks;
        private readonly MemoryMappedViewAccessor[] _accessors;

        private const long PlotSizeOffset = 2 * sizeof(int);
        private const long HitCountSize = sizeof(int);

        public Size Resolution { get; private set; }

        public static MemoryMappedHitPlot OpenForSaving(string filePath, Size resolution)
        {
            return new MemoryMappedHitPlot(filePath, resolution);
        }

        private MemoryMappedHitPlot(string filePath, Size resolution)
        {
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

            using (var plotSizeAccessor = _file.CreateViewAccessor())
            {
                plotSizeAccessor.Write(0, Resolution.Width);
                plotSizeAccessor.Write(sizeof(int), Resolution.Height);
            }

            _accessorLocks = Enumerable.Range(0, SegmentCount).Select(_ => new object()).ToArray();
            SegmentSize = dataSize / SegmentCount;
            _accessors =
                Enumerable.Range(0, SegmentCount).
                Select(i => PlotSizeOffset + i * SegmentSize).
                Select(offset => _file.CreateViewAccessor(offset, SegmentSize)).
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
            using (var plotSizeAccessor = _file.CreateViewAccessor())
            {
                var width = plotSizeAccessor.ReadInt32(0);
                var height = plotSizeAccessor.ReadInt32(sizeof(int));

                Resolution = new Size(width, height);
            }

            long dataSize = (long)Resolution.Width * (long)Resolution.Height * HitCountSize;

            _accessorLocks = Enumerable.Range(0, SegmentCount).Select(_ => new object()).ToArray();
            SegmentSize = dataSize / SegmentCount;
            _accessors =
                Enumerable.Range(0, SegmentCount).
                Select(i => PlotSizeOffset + i * SegmentSize).
                Select(offset => _file.CreateViewAccessor(offset, SegmentSize)).
                ToArray();
        }

        public void Dispose()
        {
            for (int i = 0; i < SegmentCount; i++)
            {
                _accessors[i].Dispose();
            }
            _file.Dispose();
        }

        public void IncrementPoint(Point p)
        {
            var position = PointToPosition(p);
            var segmentIndex = PositionToSegmentIndex(position);
            var segmentPosition = position % SegmentSize;

            lock (_accessorLocks[segmentIndex])
            {
                var currentCount = _accessors[segmentIndex].ReadInt32(segmentPosition);
                currentCount++;
                _accessors[segmentIndex].Write(segmentPosition, currentCount);
            }
        }

        public int GetHitsForPoint(Point p)
        {
            var position = PointToPosition(p);
            var segmentIndex = PositionToSegmentIndex(position);
            var segmentPosition = position % SegmentSize;

            return _accessors[segmentIndex].ReadInt32(segmentPosition);
        }

        private long PointToPosition(Point p)
        {
            return (long)PlotSizeOffset + (long)Resolution.Width * HitCountSize * (long)p.Y + HitCountSize * (long)p.X;
        }

        private long PositionToSegmentIndex(long position)
        {
            return (position - PlotSizeOffset) / SegmentSize;
        }

        public int Max()
        {
            return
                ParallelEnumerable.Range(0, SegmentCount).
                Select(segmentIndex =>
                {
                    var bufferCount = 100;
                    var maxes = new int[bufferCount];
                    long bufferSize = SegmentSize / bufferCount;
                    var buffer = new int[bufferSize];

                    for (int bufferIndex = 0; bufferIndex < bufferCount; bufferIndex++)
                    {
                        _accessors[segmentIndex].ReadArray(
                            position: bufferIndex * bufferSize,
                            array: buffer,
                            offset: 0,
                            count: (int)bufferSize);

                        maxes[bufferIndex] = buffer.Max();
                    }

                    return maxes.Max();
                }).
                Max();
        }
    }
}