using System;
using System.Collections.Generic;
using System.IO;
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
        [Ignore]
        public void ShouldPutThingsInCorrectBin()
        {
            var h = new Histogram();

            h.IncrementBin(127);
            h.IncrementBin(128);
            h.IncrementBin(129);
            h.IncrementBin(ushort.MaxValue);

            h.SaveToCsv(Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                "testhistogram.csv"));
        }
    }
}
