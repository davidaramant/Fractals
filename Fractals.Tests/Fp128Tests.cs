using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Benchmarks;
using NUnit.Framework;

namespace Fractals.Tests
{
    [TestFixture]
    public sealed class Fp128Tests
    {
        [TestCase(10, "10")]
        [TestCase(-3, "-3")]
        public void ShouldPrintCorrectValue(int x, string expected)
        {
            var fp = new Fp128(x);
            Assert.That(fp.ToString(), Is.EqualTo(expected));
        }

        [TestCase(-2, "FFFFFFFD 00000000 00000000 00000000")]
        [TestCase(-3, "FFFFFFFC 00000000 00000000 00000000")]
        public void ShouldHaveCorrectEncoding(int x, string expected)
        {
            var fp = new Fp128(x);
            Assert.That(fp.GetPrettyPrintHex(), Is.EqualTo(expected));
        }
    }
}
