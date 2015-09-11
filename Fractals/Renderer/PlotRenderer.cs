using Fractals.Arguments;
using Fractals.Utility;
using log4net;
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Fractals.Renderer
{
    public sealed class PlotRenderer
    {
        private readonly string _inputInputDirectory;
        private readonly string _inputFilename;

        private readonly Size _resolution;

        private static ILog _log;

        private const int TileSize = 200;

        //private readonly IHitPlot _hitPlot;
        private readonly MemoryMappedHitPlot _hitPlot;

        public PlotRenderer(string inputDirectory, string inputFilename, int width, int height)
        {
            _inputInputDirectory = inputDirectory;
            _inputFilename = inputFilename;

            _resolution = new Size(width, height);

            //            _hitPlot = new HitPlot4x4(_resolution);
            _hitPlot = MemoryMappedHitPlot.OpenForReading(Path.Combine(_inputInputDirectory, _inputFilename));

            _log = LogManager.GetLogger(GetType());
        }


        public void Render(string outputDirectory, string outputFilename)
        {
            Render(outputDirectory, outputFilename, ColorRampFactory.Blue);
        }

        public void Render(string outputDirectory, string outputFilename, ColorRamp colorRamp)
        {
            _log.InfoFormat("Creating image ({0:N0}x{1:N0})", _resolution.Width, _resolution.Height);

            _log.Info("Loading trajectory...");

            //_hitPlot.LoadTrajectories(Path.Combine(_inputInputDirectory, _inputFilename));

            _log.Info("Done loading; finding maximum...");

            var max = _hitPlot.GetMax();

            _log.DebugFormat("Found maximum: {0:N0}", max);

            _log.Info("Starting to render");

            var rows = _resolution.Height / TileSize;
            var cols = _resolution.Width / TileSize;

            //Histogram totalHistogram =
            //    ParallelEnumerable.Range(0, rows).
            //    Select(
            //        rowIndex =>
            //        {
            //            var histogram = new Histogram(max);
            //            for (int y = 0; y < TileSize; y++)
            //            {
            //                for (int x = 0; x < _resolution.Width; x++)
            //                {
            //                    var pointInPlot = new Point(x, rowIndex * TileSize + y);

            //                    var current = _hitPlot.GetHitsForPoint(pointInPlot);

            //                    histogram.IncrementBin(current);
            //                }
            //            }

            //            _log.Info($"Done with row index {rowIndex}");

            //            return histogram;
            //        }).Aggregate(new Histogram(max), (a, b) => a + b);

            //totalHistogram.SaveToCsv("histogram.csv");

            //_hitPlot.Dispose();
            //_log.Info("Done getting histogram!");
            //return;

            Parallel.For(0, rows, rowIndex =>
            {
                var outputDir = Path.Combine(outputDirectory, rowIndex.ToString());
                Directory.CreateDirectory(outputDir);

                for (int columnIndex = 0; columnIndex < cols; columnIndex++)
                {
                    using (var tile = new Bitmap(TileSize, TileSize))
                    {
                        for (int y = 0; y < TileSize; y++)
                        {
                            for (int x = 0; x < TileSize; x++)
                            {
                                var pointInPlot = new Point(columnIndex * TileSize + x, rowIndex * TileSize + y);

                                var current = _hitPlot.GetHitsForPoint(pointInPlot);

                                var ratio = Gamma(1.0 - Math.Pow(Math.E, -10.0 * current / max));

                                var color = colorRamp.GetColor(ratio).ToColor();
                                tile.SetPixel(x, y, color);
                            }
                        }

                        tile.Save(
                            Path.Combine(outputDir, columnIndex + ".png"));
                    }
                }
            });

            //var outputImg = new Bitmap(_resolution.Width, _resolution.Height);

            //var processedPixels =
            //    _resolution
            //    .GetAllPoints()
            //    .AsParallel()
            //    .WithDegreeOfParallelism(GlobalArguments.DegreesOfParallelism)
            //    .Select(p => ComputeColor(p, max, colorRamp))
            //    .AsEnumerable();

            //foreach (var result in processedPixels)
            //{
            //    outputImg.SetPixel(result.Item1.X, result.Item1.Y, result.Item2);
            //}

            _hitPlot.Dispose();

            _log.Info("Finished rendering");

            _log.Debug("Saving image");
            //outputImg.Save(Path.Combine(outputDirectory, outputFilename + ".png"));
            _log.Debug("Done saving image");

        }

        private Tuple<Point, Color> ComputeColor(Point p, int max, ColorRamp colorRamp)
        {
            var current = _hitPlot.GetHitsForPoint(p);

            var ratio = Gamma(1.0 - Math.Pow(Math.E, -10.0 * current / max));

            return Tuple.Create(p, colorRamp.GetColor(ratio).ToColor());
        }

        private static double Gamma(double x, double exp = 1.2)
        {
            return Math.Pow(x, 1.0 / exp);
        }
    }
}
