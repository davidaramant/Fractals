using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fractals.Model;
using NUnit.Framework;

namespace Fractals.Tests
{
    [TestFixture]
    public sealed class ComplexMathSpeedTest
    {
        [Test]
        [Repeat(10)]
        public void TestComplexVsManualMath()
        {
            const int bailout = 300000;

            var c = new Complex(-0.5, 0);

            double junk = 0;

            GC.Collect(2);

            var complexTimer = Stopwatch.StartNew();

            var z = c;

            for (int i = 0; i < bailout; i++)
            {
                z = z * z + c;

                junk += z.MagnitudeSquared();
            }

            complexTimer.Stop();

            GC.Collect(2);

            junk = 0;

            var manualTimer = Stopwatch.StartNew();

            var rePrev = c.Real;
            var imPrev = c.Imag;

            double re = 0;
            double im = 0;

            for (int i = 0; i < bailout; i++)
            {
                var reTemp = re * re - im * im + rePrev;
                im = 2 * re * im + imPrev;
                re = reTemp;

                junk += re * re + im * im;
            }

            manualTimer.Stop();

            Assert.That( manualTimer.Elapsed, Is.LessThan(complexTimer.Elapsed),"Manual math should be faster");

            Console.Out.WriteLine("Manual: {0}\tComplex: {1}",manualTimer.ElapsedTicks,complexTimer.ElapsedTicks);
        }
    }
}
