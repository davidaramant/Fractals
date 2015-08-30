using System.Threading;
using Fractals.Arguments;
using Fractals.Model;
using log4net;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;

namespace Fractals.Utility
{
    public sealed class TrajectoryPlotter
    {
        private readonly string _inputDirectory;
        private readonly string _inputFilenamePattern;
        private readonly string _outputDirectory;
        private readonly string _outputFilename;
        private readonly Size _resolution;
        private readonly uint _bailout;

        //private readonly IHitPlot _hitPlot;
        private readonly MemoryMappedHitPlot _hitPlot;

        private static ILog _log;

        public TrajectoryPlotter(string inputDirectory, string inputFilenamePattern, string outputDirectory, string outputFilename, int width, int height, uint bailout)
        {
            _inputDirectory = inputDirectory;
            _inputFilenamePattern = inputFilenamePattern;
            _outputDirectory = outputDirectory;
            _outputFilename = outputFilename;
            _resolution = new Size(width, height);
            _bailout = bailout;

            _log = LogManager.GetLogger(GetType());

            if (width % 4 != 0)
            {
                _log.Warn("The width should be evenly divisible by 4");
            }
            if (height % 4 != 0)
            {
                _log.Warn("The height should be evenly divisible by 4");
            }

            //_hitPlot = new HitPlot4x4(_resolution);
            _hitPlot = MemoryMappedHitPlot.OpenForSaving(Path.Combine(_outputDirectory, _outputFilename), _resolution);
        }

        public void Plot()
        {
            _log.InfoFormat("Plotting image ({0:N0}x{1:N0})", _resolution.Width, _resolution.Height);
            _log.DebugFormat("Iterating {0:N0} times per point", _bailout);

            var viewPort = AreaFactory.RenderingArea;

            viewPort.LogViewport();

            var rotatedResolution = new Size(_resolution.Height, _resolution.Width);

            _log.Info("Calculating trajectories");

            var processedCount = 0;
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

                Interlocked.Increment(ref processedCount);
                if (processedCount % 100 == 0)
                {
                    _log.DebugFormat("Plotted {0:N0} points' trajectories", processedCount);
                }
            });
            _hitPlot.Dispose();
            _log.DebugFormat("Plotted {0:N0} points' trajectories", processedCount);

            _log.Info("Done plotting trajectories");
            //_log.DebugFormat("Maximum point hit count: {0:N0}", _hitPlot.Max());

            //_hitPlot.SaveTrajectories(Path.Combine(_outputDirectory, _outputFilename));

            _log.DebugFormat("Saved plot as: {0}", _outputFilename);
        }

        private IEnumerable<Complex> GetNumbers()
        {
            var list = new ComplexNumberListReader(_inputDirectory, _inputFilenamePattern);
            return list.GetNumbers();
        }

        private IEnumerable<Complex> GetTrajectory(Complex c)
        {
            double re = 0;
            double im = 0;

            // Cache the squares
            // They are used to find the magnitude; reuse these values when computing the next re/im
            double re2 = 0;
            double im2 = 0;

            for (uint i = 0; i < _bailout; i++)
            {
                var reTemp = re2 - im2 + c.Real;
                im = 2 * re * im + c.Imaginary;
                re = reTemp;

                yield return new Complex(re, im);

                re2 = re * re;
                im2 = im * im;

                // Check the magnitude squared against 2^2
                if ((re2 + im2) > 4)
                {
                    yield break;
                }
            }
        }

    }
}