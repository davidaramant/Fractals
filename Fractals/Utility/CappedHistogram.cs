using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Fractals.Utility
{
    sealed class CappedHistogram : IHistogram
    {
        private readonly ushort _max;
        private readonly ulong[] _bins;

        public CappedHistogram(ushort max)
        {
            _max = max;
            _bins = new ulong[max];
        }

        public IEnumerator<ulong> GetEnumerator()
        {
            return (IEnumerator<ulong>) _bins.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void IncrementBin(ushort value)
        {
            var bin = Math.Min(value, _max - 1);
            _bins[bin]++;
        }

        public void SaveToCsv(string filePath)
        {
            File.WriteAllLines(
                filePath,
                new[] { $"Capped Max: {_max}", "Bin,Count" }.
                Concat(_bins.Select((value, index) => $"{index},{value}"))
                );
        }
    }
}
