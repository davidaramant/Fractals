using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace Fractals.Utility
{
    public sealed class Histogram : IEnumerable<ulong>
    {
        public const int Count = 100;
        private readonly ulong[] _bins = new ulong[Count];
        private ulong _zeroBin;
        private readonly ushort _max;

        public int BinSize { get; }

        public Histogram(ushort maxValue)
        {
            _max = maxValue;
            BinSize = maxValue / Count;
        }

        private Histogram(int binSize, ushort maxValue, ulong zeroBin, ulong[] bins)
        {
            BinSize = binSize;
            _max = maxValue;
            _zeroBin = zeroBin;
            _bins = bins;
        }

        public void IncrementBin(int value)
        {
            if (value == 0)
            {
                _zeroBin++;
            }
            else
            {
                var index = value / BinSize;
                _bins[Math.Min(index, Count - 1)]++;
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
                binSize: h1.BinSize,
                maxValue: h1._max,
                zeroBin: h1._zeroBin + h2._zeroBin,
                bins: added);
        }

        public void SaveToCsv(string filePath)
        {
            File.WriteAllLines(
                filePath,
                new[] { "Min,Max,Count", $"0,0,{_zeroBin}", $"1,{BinSize - 1},{_bins[0]}" }.
                Concat(this.Skip(1).Take(Count - 2).Select((value, index) => $"{(index + 1) * BinSize},{(index + 2) * BinSize - 1},{value}")).
                Concat(new[] { $"{(Count - 1) * BinSize},{_max},{_bins[Count - 1]}" })
                );
        }

        public IEnumerator<ulong> GetEnumerator()
        {
            return ((IEnumerable<ulong>)_bins).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}