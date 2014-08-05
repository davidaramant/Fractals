using System;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using Fractals.Utility;
using NUnit.Framework;

namespace Fractals.Tests.Utility
{
    [TestFixture]
    public class HitPlotTests
    {
        [Test]
        public void ReversibilityMax()
        {
            var tempPath = Path.GetTempFileName();

            try
            {
                var generatedPlot = GenerateHitPlot();

                generatedPlot.SaveTrajectories(tempPath);

                var loadedPlot = new HitPlot4x4(generatedPlot.Resolution);
                loadedPlot.LoadTrajectories(tempPath);

                Assert.That(loadedPlot.Max(), Is.EqualTo(generatedPlot.Max()));
            }
            finally
            {
                File.Delete(tempPath);
            }
        }

        [Test]
        public void ReversibilityPoints()
        {
            var tempPath = Path.GetTempFileName();

            try
            {
                var generatedPlot = GenerateHitPlot();

                generatedPlot.SaveTrajectories(tempPath);

                var loadedPlot = new HitPlot4x4(generatedPlot.Resolution);
                loadedPlot.LoadTrajectories(tempPath);

                foreach (var point in generatedPlot.Resolution.GetAllPoints())
                {
                    Assert.That(loadedPlot.GetHitsForPoint(point), Is.EqualTo(generatedPlot.GetHitsForPoint(point)));
                }
            }
            finally
            {
                File.Delete(tempPath);
            }
        }

        [Test]
        public void FilesAreSeparate()
        {
            var tempPath1 = Path.GetTempFileName();
            var tempPath2 = Path.GetTempFileName();

            try
            {
                var generatedPlot1 = GenerateHitPlot();
                generatedPlot1.SaveTrajectories(tempPath1);

                var generatedPlot2 = GenerateHitPlot();
                generatedPlot2.SaveTrajectories(tempPath2);

                var loadedPlot1 = new HitPlot4x4(generatedPlot1.Resolution);
                loadedPlot1.LoadTrajectories(tempPath1);

                var loadedPlot2 = new HitPlot4x4(generatedPlot2.Resolution);
                loadedPlot2.LoadTrajectories(tempPath2);

                Assert.That(loadedPlot1.Max(), Is.Not.EqualTo(loadedPlot2.Max()));
            }
            finally
            {
                File.Delete(tempPath1);
                File.Delete(tempPath2);
            }
        }

        private HitPlot4x4 GenerateHitPlot()
        {
            const int size = 1024;
            var hitPlot = new HitPlot4x4(new Size(size, size));

            var random = new Random();

            Parallel.ForEach(hitPlot.Resolution.GetAllPoints(), point =>
            {
                for (int i = 0; i < random.Next(1000); i++)
                {
                    hitPlot.IncrementPoint(point);
                }
            });

            return hitPlot;
        }
    }
}