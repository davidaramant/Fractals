using System.IO;
using System.Linq;
using Fractals.Model;
using Fractals.Utility;
using NUnit.Framework;

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
                new Complex(1, 1),
                new Complex(0.25, 0.5)
            };

            string path = Path.GetTempFileName();
            var file = new FileInfo(path);

            try
            {
                var list = new ComplexNumberList(file.DirectoryName, file.Name);

                foreach (Complex n in numbers)
                {
                    list.SaveNumber(n);
                }

                Complex[] roundTripped = list.GetNumbers().ToArray();

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