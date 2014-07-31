using Fractals.Utility;
using NUnit.Framework;
using System;

namespace Fractals.Tests.Utility
{
    [TestFixture]
    public sealed class ReallyRandomTests
    {
        [Test]
        public void CheckDistribution()
        {
            const int trials = 10000000;
            const int samples = 20;

            const int ideal = trials / samples;

            var mathRand = new Random();
            var mathRandDistribution = GetDistribution(trials, samples, mathRand.NextDouble);

            var reallyRand = new ReallyRandom();
            var reallyRandDistribution = GetDistribution(trials, samples, reallyRand.Next);

            Console.WriteLine("Distribution of Random Numbers:");
            for (int ctr = 0; ctr < samples; ctr++)
            {
                var mathOffset = ideal - mathRandDistribution[ctr];
                var reallyOffset = ideal - reallyRandDistribution[ctr];

                Console.WriteLine("{0,6} {1,6}", mathOffset, reallyOffset);
            }
        }

        private static int[] GetDistribution(int trials, int samples, Func<double> nextDouble)
        {
            int[] frequency = new int[samples];

            for (int ctr = 0; ctr <= trials; ctr++)
            {
                var number = nextDouble();
                frequency[(int)Math.Floor(number * samples)]++;
            }

            return frequency;
        }
    }
}
