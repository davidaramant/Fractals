using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Fractals.Utility;
using Fractals.Model;

namespace Fractals.Tests.Utility
{
    [TestFixture]
    public sealed class ComplexNumberListTests
    {
        [Test]
        public void ShouldRoundTripNumbers()
        {
            var numbers = new[]
            {
                new Complex(),
                new Complex(1,1), 
                new Complex(0.25,0.5), 
            };

            var path = System.IO.Path.GetTempFileName();

            try
            {
                var list = new ComplexNumberList(path);

                foreach (var n in numbers)
                {
                    list.SaveNumber(n);
                }

                var roundTripped = list.GetNumbers().ToArray();

                Assert.That(
                    roundTripped.Select(n => n.ToString()).ToArray(), 
                    Is.EquivalentTo(numbers.Select(n => n.ToString()).ToArray()),
                    "Did not save and load numbers.");
            }
            finally
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }
        }

    }
}
