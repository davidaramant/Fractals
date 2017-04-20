using System;
using Fractals.Model;

namespace Fractals.Utility
{
    public static class RandomExtensions
    {
        public static double Next(this Random random, InclusiveRange range)
        {
            return range.Magnitude * random.NextDouble() + range.Minimum;
        }
    }
}
