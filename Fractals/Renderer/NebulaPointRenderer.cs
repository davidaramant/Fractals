using System;
using System.Drawing;
using System.IO;
using System.Linq;
using Fractals.Arguments;
using Fractals.Utility;
using log4net;

namespace Fractals.Renderer
{
    public class NebulaPointRenderer
    {
        private readonly string _inputInputDirectory;
        private readonly string _inputFilenameRed;
        private readonly string _inputFilenameGreen;
        private readonly string _inputFilenameBlue;

        private readonly Size _resolution;

        private static ILog _log;

        private readonly HitPlot _hitPlotRed;
        private readonly HitPlot _hitPlotGreen;
        private readonly HitPlot _hitPlotBlue;

        public NebulaPointRenderer(string inputDirectory, string inputFilenameRed, string inputFilenameGreen, string inputFilenameBlue, int width, int height)
        {
            _inputInputDirectory = inputDirectory;
            _inputFilenameRed = inputFilenameRed;
            _inputFilenameGreen = inputFilenameGreen;
            _inputFilenameBlue = inputFilenameBlue;

            _resolution = new Size(width, height);

            _hitPlotRed = new HitPlot(_resolution);
            _hitPlotGreen = new HitPlot(_resolution);
            _hitPlotBlue = new HitPlot(_resolution);

            _log = LogManager.GetLogger(GetType());
        }

        public void Render(string outputDirectory, string outputFilename)
        {
            _log.Info("Loading trajectories...");

            _hitPlotRed.LoadTrajectories(Path.Combine(_inputInputDirectory, _inputFilenameRed));
            _hitPlotGreen.LoadTrajectories(Path.Combine(_inputInputDirectory, _inputFilenameGreen));
            _hitPlotBlue.LoadTrajectories(Path.Combine(_inputInputDirectory, _inputFilenameBlue));

            _log.Info("Done loading; finding maximums...");

            var maxRed = _hitPlotRed.FindMaximumHit();
            var maxGreen = _hitPlotGreen.FindMaximumHit();
            var maxBlue = _hitPlotBlue.FindMaximumHit();

            _log.DebugFormat("Found maximum red: {0}", maxRed);
            _log.DebugFormat("Found maximum green: {0}", maxGreen);
            _log.DebugFormat("Found maximum blue: {0}", maxBlue);

            _log.Info("Starting to render");

            var outputImg = new Bitmap(_resolution.Width, _resolution.Height);

            var processedPixels =
                _resolution
                .GetAllPoints()
                .AsParallel()
                .WithDegreeOfParallelism(GlobalArguments.DegreesOfParallelism)
                .Select(p => ComputeColor(p, maxRed, maxGreen, maxBlue))
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

        private Tuple<Point, Color> ComputeColor(Point p, int maxRed, int maxGreen, int maxBlue)
        {
            var currentRed = _hitPlotRed.GetHitsForPoint(p);
            var currentGreen = _hitPlotGreen.GetHitsForPoint(p);
            var currentBlue = _hitPlotBlue.GetHitsForPoint(p);

            var expRed = Gamma(1.0 - Math.Pow(Math.E, -10.0 * currentRed / maxRed));
            var expGreen = Gamma(1.0 - Math.Pow(Math.E, -10.0 * currentGreen / maxGreen));
            var expBlue = Gamma(1.0 - Math.Pow(Math.E, -10.0 * currentBlue / maxBlue));

            var color = Color.FromArgb(
                (int)(255 * expRed),
                (int)(255 * expGreen),
                (int)(255 * expBlue));

            return Tuple.Create(p, color);
        }

        private static double Gamma(double x, double exp = 1.2)
        {
            return Math.Pow(x, 1.0 / exp);
        }
    }
}
