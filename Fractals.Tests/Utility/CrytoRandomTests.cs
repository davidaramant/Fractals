using System.Collections.Generic;
using System.Linq;
using Fractals.Utility;
using NUnit.Framework;
using System;

namespace Fractals.Tests.Utility
{
    [TestFixture]
    public sealed class CrytoRandomTests
    {
        [Test]
//        [Repeat(10)]
        public void SanityCheckDuplicates()
        {
            const int distributionBuckets = 100;
            const int numbersToSample = 1000000;

            var mathRandom = new Random();
            var mathResults = new List<double>();
            for (int i = 0; i < numbersToSample; i++)
            {
                mathResults.Add(mathRandom.NextDouble());
            }
            mathResults.Sort();
            var mathDistribution = GetDistribution(mathResults, distributionBuckets);
            var mathDistributedValues = mathDistribution.Values.OrderByDescending(x => x).ToList();

            var rngRandom = new CryptoRandom();
            var rngResults = new List<double>();
            for (int i = 0; i < numbersToSample; i++)
            {
                rngResults.Add(rngRandom.NextDouble());
            }
            rngResults.Sort();
            var rngDistribution = GetDistribution(rngResults, distributionBuckets);
            var rngDistributedValues = rngDistribution.Values.OrderByDescending(x => x).ToList();

            for (int i = 0; i < distributionBuckets; i++)
            {
                Console.WriteLine("{0,2}: {1,20} {2,20}", i, mathDistributedValues[i], rngDistributedValues[i]);
            }

//            var mathMin = mathDistribution.Values.Min();
//            var mathMax = mathDistribution.Values.Min();
//
//            var rngMin = rngDistribution.Values.Min();
//            var rnghMax = rngDistribution.Values.Min();
//
//            Assert.GreaterOrEqual(rngMin, mathMin);
//            Assert.LessOrEqual(rnghMax, mathMax);
        }

        private Dictionary<int, int> GetDistribution(IEnumerable<double> inputs, int distributionBuckets)
        {
            var results = new Dictionary<int, int>();

            for (var bucketIndex = 0; bucketIndex < distributionBuckets; bucketIndex++)
            {
                results.Add(bucketIndex, 0);
            }

            foreach (var input in inputs)
            {
                var shifted = (int)(input * distributionBuckets);
                results[shifted] ++;
            }

            return results;
        }
    }
}
