
using System;
using System.Numerics;

namespace Benchmarks
{
    public static class VectorExtensions
    {
        public static int Count(this Vector<int> vector, Func<int, bool> counter)
        {
            int count = 0;
            for (int i = 0; i < Vector<int>.Count; i++)
            {
                if (counter(vector[i]))
                {
                    count++;
                }
            }
            return count;
        }
    }
}
