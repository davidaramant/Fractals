using System;
using System.IO;
using System.Linq;
using Fractals.Model;
using Fractals.Utility;
using NUnit.Framework;

namespace Fractals.Tests.Utility
{
    [TestFixture]
    class AreaListTests
    {
        [Test]
        public void ShouldRoundTripNumbers()
        {
            var areas = new[]
            {
                new Area(new InclusiveRange(0, 0), new InclusiveRange(0, 0)),
                new Area(new InclusiveRange(0, 1), new InclusiveRange(3, 4)),
                new Area(new InclusiveRange(-0.25, 0.75), new InclusiveRange(-0.15, 0.85))
            };

            string path = Path.GetTempFileName();
            var file = new FileInfo(path);

            try
            {
                var listWriter = new AreaListWriter(file.DirectoryName, file.Name);

                foreach (Area a in areas)
                {
                    listWriter.SaveArea(a);
                }

                var listReader = new AreaListReader(file.DirectoryName, file.Name);

                Area[] roundTripped = listReader.GetAreas().ToArray();

                Assert.That(roundTripped.Length, Is.EqualTo(areas.Length));

                foreach (var roundTripArea in roundTripped)
                {
                    Assert.That(areas.Any(a =>
                        (Math.Abs(a.RealRange.Minimum - roundTripArea.RealRange.Minimum) < 0.001) &&
                        (Math.Abs(a.RealRange.Minimum - roundTripArea.RealRange.Minimum) < 0.001) &&
                        (Math.Abs(a.RealRange.Minimum - roundTripArea.RealRange.Minimum) < 0.001) &&
                        (Math.Abs(a.RealRange.Minimum - roundTripArea.RealRange.Minimum) < 0.001)));
                }
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
