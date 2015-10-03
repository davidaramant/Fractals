using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace Fractals.Utility
{
    public sealed class Histogram : IEnumerable<ulong>
    {
        public const ushort Count = 512;
        private readonly ulong[] _bins = new ulong[Count];
        private ulong _zeroBin;
        private ushort _max;

        public const ushort BinSize = (int)(ushort.MaxValue + 1) / Count;

        public Histogram()
        {
        }

        private Histogram(ushort maxValue, ulong zeroBin, ulong[] bins)
        {
            _max = maxValue;
            _zeroBin = zeroBin;
            _bins = bins;
        }

        public void IncrementBin(ushort value)
        {
            if (value == 0)
            {
                _zeroBin++;
            }
            else
            {
                var index = value / BinSize;
                _bins[index]++;
                _max = Math.Max(_max, value);
            }
        }

        public static Histogram operator +(Histogram h1, Histogram h2)
        {
            var added = new ulong[Count];

            for (int i = 0; i < Count; i++)
            {
                added[i] = h1._bins[i] + h2._bins[i];
            }

            return new Histogram(
                maxValue: Math.Max(h1._max, h2._max),
                zeroBin: h1._zeroBin + h2._zeroBin,
                bins: added);
        }

        public void SaveToCsv(string filePath)
        {
            File.WriteAllLines(
                filePath,
                new[] { $"Max: {_max}", "Min,Max,Count", $"0,0,{_zeroBin}", $"1,{BinSize - 1},{_bins[0]}" }.
                Concat(_bins.Skip(1).Select((value, index) => $"{(index + 1) * BinSize},{(index + 2) * BinSize - 1},{value}"))
                );
        }

        public IEnumerator<ulong> GetEnumerator()
        {
            return new[] { _zeroBin }.Concat(_bins).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}