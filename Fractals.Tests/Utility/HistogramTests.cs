using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Fractals.Utility;

namespace Fractals.Tests.Utility
{
    [TestFixture]
    public class HistogramTests
    {
        [Test]
        public void ShouldCountZeros()
        {
            var h = new Histogram();
            h.IncrementBin(0);
            h.IncrementBin(0);
            h.IncrementBin(0);

            Assert.That( h.First(), Is.EqualTo(3));
        }

        [Test]
        public void ShouldHandleMaximum()
        {
            var h = new Histogram();

            h.IncrementBin(ushort.MaxValue);

            Assert.That(h.Last(),Is.EqualTo(1));
        }

        [Test]
        public void ShouldPutThingsInCorrectBin()
        {
            var h = new Histogram();

            h.IncrementBin(255);
            h.IncrementBin(256);
            h.IncrementBin(257);

            Assert.That(h.ElementAt(1), Is.EqualTo(1));
            Assert.That(h.ElementAt(2), Is.EqualTo(2));
        }
    }
}
