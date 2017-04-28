using System;

namespace Benchmarks
{
    public static class Util
    {
        public static (int aTotal, int bTotal) Partition(int total, (int a, int b) ratio, (int a, int b) batchSize)
        {
            var commonBatch = LeastCommonMultiple(batchSize.a, batchSize.b);

            if (total % commonBatch != 0)
            {
                throw new ArgumentException($"Cannot divide {total} into sets of {commonBatch}");
            }

            var totalBatches = total / commonBatch;

            var ratioTotal = ratio.a + ratio.b;

            bool aBigger = ratio.a > ratio.b;

            var smallerRatioPortion = Math.Min(ratio.a, ratio.b);
            var desiredSmallPercentage = (double)smallerRatioPortion / ratioTotal;

            var smallBatchNumber = (int)(desiredSmallPercentage * totalBatches);
            var bigBatchNumber = totalBatches - smallBatchNumber;

            var smallTotal = smallBatchNumber * commonBatch;
            var bigTotal = bigBatchNumber * commonBatch;

            return aBigger ?
                (bigTotal, smallTotal) :
                (smallTotal, bigTotal);
        }

        public static int LeastCommonMultiple(int a, int b)
        {
            return (a / GreatestCommonFactor(a, b)) * b;
        }

        public static int GreatestCommonFactor(int a, int b)
        {
            while (b != 0)
            {
                int temp = b;
                b = a % b;
                a = temp;
            }
            return a;
        }
    }
}
