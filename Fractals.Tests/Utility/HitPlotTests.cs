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
        public void Reversibility()
        {
            var generatedPlot = GenerateHitPlot();
            var tempPath = Path.GetTempFileName();
            generatedPlot.SaveTrajectories(tempPath);

            var loadedPlot = new HitPlot4x4(generatedPlot.Resolution);
            loadedPlot.LoadTrajectories(tempPath);

            Assert.That(loadedPlot.Max(), Is.EqualTo(generatedPlot.Max()));
            Assert.That(loadedPlot.Sum(), Is.EqualTo(generatedPlot.Sum()));

            File.Delete(tempPath);
        }

        [Test]
        public void FilesAreSeparate()
        {
            var generatedPlot1 = GenerateHitPlot();
            var tempPath1 = Path.GetTempFileName();
            generatedPlot1.SaveTrajectories(tempPath1);

            var generatedPlot2 = GenerateHitPlot();
            var tempPath2 = Path.GetTempFileName();
            generatedPlot2.SaveTrajectories(tempPath2);

            var loadedPlot1 = new HitPlot4x4(generatedPlot1.Resolution);
            loadedPlot1.LoadTrajectories(tempPath1);

            var loadedPlot2 = new HitPlot4x4(generatedPlot2.Resolution);
            loadedPlot2.LoadTrajectories(tempPath2);

            Assert.That(loadedPlot1.Max(), Is.Not.EqualTo(loadedPlot2.Max()));
            Assert.That(loadedPlot1.Sum(), Is.Not.EqualTo(loadedPlot2.Sum()));

            File.Delete(tempPath1);
            File.Delete(tempPath2);
        }

        private HitPlot4x4 GenerateHitPlot()
        {
            const int size = 2048;
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