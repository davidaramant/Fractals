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

        public int BinSize { get; }

        public Histogram(int maxValue)
        {
            BinSize = maxValue / Count;
        }

        private Histogram(int binSize, ulong[] bins)
        {
            BinSize = binSize;
            _bins = bins;
        }

        public void IncrementBin(int value)
        {
            var index = value / BinSize;
            _bins[Math.Min(index, Count - 1)]++;
        }

        public static Histogram operator +(Histogram h1, Histogram h2)
        {
            var added = new ulong[Count];

            for (int i = 0; i < Count; i++)
            {
                added[i] = h1._bins[i] + h2._bins[i];
            }

            return new Histogram(h1.BinSize, added);
        }

        public void SaveToCsv(string filePath)
        {
            File.WriteAllText(
                filePath,
                String.Join(
                    Environment.NewLine, 
                    this.Select((value, index) => $"\"{index * BinSize} - {(index + 1) * BinSize}\",{value}")));
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