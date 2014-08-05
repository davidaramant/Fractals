using Fractals.Arguments;
using Fractals.Utility;
using log4net;
using System;
using System.Drawing;
using System.IO;
using System.Linq;

namespace Fractals.Renderer
{
    public sealed class PlotRenderer
    {
        private readonly string _inputInputDirectory;
        private readonly string _inputFilename;

        private readonly Size _resolution;

        private static ILog _log;

        private readonly IHitPlot _hitPlot;

        private readonly ColorRamp _colorRamp = ColorRampFactory.Blue;

        public PlotRenderer(string inputDirectory, string inputFilename, int width, int height)
        {
            _inputInputDirectory = inputDirectory;
            _inputFilename = inputFilename;

            _resolution = new Size(width, height);

            _hitPlot = new HitPlot4x4(_resolution);

            _log = LogManager.GetLogger(GetType());
        }

        public void Render(string outputDirectory, string outputFilename)
        {
            _log.InfoFormat("Creating image ({0}x{1})", _resolution.Width, _resolution.Height);

            _log.Info("Loading trajectory...");

            _hitPlot.LoadTrajectories(Path.Combine(_inputInputDirectory, _inputFilename));

            _log.Info("Done loading; finding maximum...");

            var max = _hitPlot.FindMaximumHit();

            _log.DebugFormat("Found maximum: {0}", max);

            _log.Info("Starting to render");

            var outputImg = new Bitmap(_resolution.Width, _resolution.Height);

            var processedPixels =
                _resolution
                .GetAllPoints()
                .AsParallel()
                .WithDegreeOfParallelism(GlobalArguments.DegreesOfParallelism)
                .Select(p => ComputeColor(p, max))
                .AsEnumerable();

            foreach (var result in processedPixels)
            {
                outputImg.SetPixel(result.Item1.X, result.Item1.Y, result.Item2);
            }

            _log.Info("Finished rendering");

            _log.Debug("Saving image");
            outputImg.Save(Path.Combine(outputDirectory, String.Format("{0}.png", outputFilename)));
            _log.Debug("Done saving image");

        }

        private Tuple<Point, Color> ComputeColor(Point p, int max)
        {
            var current = _hitPlot.GetHitsForPoint(p);

            var ratio = Gamma(1.0 - Math.Pow(Math.E, -10.0 * current / max));

            return Tuple.Create(p, _colorRamp.GetColor(ratio).ToColor());
        }

        private static double Gamma(double x, double exp = 1.2)
        {
            return Math.Pow(x, 1.0 / exp);
        }
    }
}
