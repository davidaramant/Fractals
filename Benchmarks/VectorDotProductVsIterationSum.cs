using System.Linq;
using System.Numerics;
using BenchmarkDotNet.Attributes;

namespace Benchmarks
{
    public class VectorDotProductVsIterationSum
    {
        [Benchmark]
        public int IterationSum()
        {
            var input = Enumerable.Range(0, Vector<int>.Count).ToArray();
            var v = new Vector<int>(input);

            int sum = 0;
            for (int i = 0; i < Vector<int>.Count; i++)
            {
                sum += v[i];
            }

            return sum;
        }

        [Benchmark]
        public int DotProductSum()
        {
            var input = Enumerable.Range(0, Vector<int>.Count).ToArray();
            var v = new Vector<int>(input);

            return Vector.Dot(v, Vector<int>.One);
        }
    }
}
