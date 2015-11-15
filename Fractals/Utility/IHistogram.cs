using System.Collections.Generic;

namespace Fractals.Utility
{
    public interface IHistogram : IEnumerable<ulong>
    {
        void IncrementBin(ushort value);
        void SaveToCsv(string filePath);
    }
}