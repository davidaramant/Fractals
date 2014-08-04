using Fractals.Arguments;
using Fractals.Model;
using log4net;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;

namespace Fractals.Utility
{
    public sealed class Plotter
    {
        private const int Bailout = 30000;

        private readonly string _inputDirectory;
        private readonly string _inputFilenamePattern;
        private readonly string _outputDirectory;
        private readonly string _outputFilename;
        private readonly Size _resolution;

        private readonly HitPlot _hitPlot;

        private static ILog _log;

        public Plotter(string inputDirectory, string inputFilenamePattern, string outputDirectory, string outputFilename, int width, int height)
        {
            _inputDirectory = inputDirectory;
            _inputFilenamePattern = inputFilenamePattern;
            _outputDirectory = outputDirectory;
            _outputFilename = outputFilename;
            _resolution = new Size(width, height);

            _log = LogManager.GetLogger(GetType());

            if (width % 4 != 0)
            {
                _log.Warn("The width should be evenly divisible by 4");
            }
            if (height % 4 != 0)
            {
                _log.Warn("The height should be evenly divisible by 4");
            }

            _hitPlot = new HitPlot(_resolution);
        }

        public void Plot()
        {
            _log.InfoFormat("Plotting image ({0}x{1})", _resolution.Width, _resolution.Height);

            var viewPort = new Area(
                            realRange: new InclusiveRange(-1.75, 1),
                            imaginaryRange: new InclusiveRange(-1.3, 1.3));

            viewPort.LogViewport();

            var rotatedResolution = new Size(_resolution.Height, _resolution.Width);

            _log.Info("Calculating trajectories");

            Parallel.ForEach(GetNumbers(), new ParallelOptions { MaxDegreeOfParallelism = GlobalArguments.DegreesOfParallelism }, number =>
            {
                foreach (var c in GetTrajectory(number))
                {
                    var point = viewPort.GetPointFromNumber(rotatedResolution, c).Rotate();

                    if (!_resolution.IsInside(point))
                    {
                        continue;
                    }

                    _hitPlot.IncrementPoint(point);
                }
            });

            _log.Info("Done plotting trajectories");

            _hitPlot.SaveTrajectories(Path.Combine(_outputDirectory, _outputFilename));
        }

        private IEnumerable<Complex> GetNumbers()
        {
            var list = new ComplexNumberListReader(_inputDirectory, _inputFilenamePattern);
            return list.GetNumbers();
        }

        private IEnumerable<Complex> GetTrajectory(Complex c)
        {
            var rePrev = c.Real;
            var imPrev = c.Imaginary;

            double re = 0;
            double im = 0;

            for (int i = 0; i < Bailout; i++)
            {
                var reTemp = re * re - im * im + rePrev;
                im = 2 * re * im + imPrev;
                re = reTemp;

                yield return new Complex(re, im);

                var magnitudeSquared = re * re + im * im;
                if (magnitudeSquared > 4)
                {
                    yield break;
                }
            }
        }

    }
}