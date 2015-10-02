using Fractals.Arguments;
using Fractals.Utility;
using log4net;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Fractals.Renderer
{
    public sealed class PlotRenderer
    {
        private readonly string _inputInputDirectory;
        private readonly string _inputFilename;

        private readonly Size _resolution;

        private static ILog _log;

        private const int TileSize = 256;


        public PlotRenderer(string inputDirectory, string inputFilename, int width, int height)
        {
            _inputInputDirectory = inputDirectory;
            _inputFilename = inputFilename;

            _resolution = new Size(width, height);

            _log = LogManager.GetLogger(GetType());
        }


        public void Render(string outputDirectory, string outputFilename)
        {
            //Render( outputDirectory, outputFilename, ColorRampFactory.Blue );
            Render(outputDirectory, outputFilename, ColorRampFactory.Psychadelic);
        }

        public void Render(string outputDirectory, string outputFilename, ColorRamp colorRamp)
        {
            var timer = Stopwatch.StartNew();

            _log.InfoFormat("Creating image ({0:N0}x{1:N0})", _resolution.Width, _resolution.Height);

            _log.Info("Loading trajectory...");

            using (var hitPlot = new HitPlotReader(Path.Combine(_inputInputDirectory, _inputFilename), _resolution))
            {
                const ushort cappedMax = 2500;

                _log.Debug($"Using maximum: {cappedMax:N0}");

                _log.Info("Starting to render");

                var rows = _resolution.Height / TileSize;

                Parallel.For(0, rows,
                    new ParallelOptions { MaxDegreeOfParallelism = GlobalArguments.DegreesOfParallelism },
                    //new ParallelOptions { MaxDegreeOfParallelism = 1 },
                    rowIndex =>
                {
                    var outputDir = Path.Combine(outputDirectory, rowIndex.ToString());
                    Directory.CreateDirectory(outputDir);

                    using (var tile = new ImageTile(TileSize))
                    {
                        for (int columnIndex = 0; columnIndex < rows; columnIndex++)
                        {
                            for (int pixelIndex = 0; pixelIndex < TileSize*TileSize; pixelIndex++)
                            {
                                var current = hitPlot.GetCount(rowIndex, columnIndex, pixelIndex);

                                 current = Math.Min(current, cappedMax);

                                var ratio = Gamma(1.0 - Math.Pow(Math.E, -15.0 * current / cappedMax));
                                //var ratio = 1.0 - Math.Pow(Math.E, -5.0 * current / cappedMax);

                                var color = colorRamp.GetColor(ratio).ToColor();
                                tile.SetPixel(pixelIndex, color);
                            }
                            tile.Save(Path.Combine(outputDir, columnIndex + ".png"));

                        }
                    }
                });
            }

            timer.Stop();

            _log.Info($"Finished rendering tiles. Took: {timer.Elapsed}");
        }

        private static double Gamma(double x, double exp = 1.2)
        {
            return Math.Pow(x, 1.0 / exp);
        }

        private void ComputeHistogram(int rows, ushort max, HitPlotReader hitPlotWriter)
        {
            Histogram totalHistogram =
                ParallelEnumerable.Range(0, rows).
                Select(
                    rowIndex =>
                    {
                        var histogram = new Histogram(max);
                        for (int y = 0; y < TileSize; y++)
                        {
                            for (int x = 0; x < _resolution.Width; x++)
                            {
                                //HACK
                                var pointInPlot = new Point(x, rowIndex * TileSize + y);

                                var current = 0;//hitPlotWriter.GetHitsForPoint(pointInPlot);

                                histogram.IncrementBin(current);
                            }
                        }

                        _log.Info($"Done with row index {rowIndex}");

                        return histogram;
                    }).Aggregate(new Histogram(max), (a, b) => a + b);

            totalHistogram.SaveToCsv("bighistogram.csv");

            _log.Info("Done getting histogram!");
        }

        private void RenderSomeTiles(HitPlotReader hitPlotWriter, ushort cappedMax, ColorRamp colorRamp, string outputDirectory)
        {
            var tilesToRender = new[]
            {
                Tuple.Create(250,125),
                Tuple.Create(100,223),
                Tuple.Create(100,228),
                Tuple.Create(100,229),
                Tuple.Create(100,230),
                Tuple.Create(100,231),
                Tuple.Create(100,298),
            };

            Parallel.For(0, tilesToRender.Length, tileIndex =>
           {
               var tileCoordinate = tilesToRender[tileIndex];
               var row = tileCoordinate.Item1;
               var col = tileCoordinate.Item2;

               using (var tile = new ImageTile(TileSize))
               {
                   for (int y = 0; y < TileSize; y++)
                   {
                       for (int x = 0; x < TileSize; x++)
                       {
                           var pointInPlot = new Point(col * TileSize + x, row * TileSize + y);

                           // HACK
                           var current = 0;//Math.Min(hitPlotWriter.GetHitsForPoint(pointInPlot), cappedMax);

                           //var ratio = Gamma(1.0 - Math.Pow(Math.E, -15.0 * current / max));
                           //var ratio = Gamma(Math.Sqrt((double)current / cappedMax));
                           //var ratio = (Math.Sqrt((double)current / cappedMax));
                           //var ratio = (double)current / cappedMax;
                           var ratio = 1.0 - Math.Pow(Math.E, -5.0 * current / cappedMax);

                           var color = colorRamp.GetColor(ratio).ToColor();
                           tile.SetPixel(x, y, color);
                       }
                   }

                   tile.Save(
                        Path.Combine(outputDirectory, $"{row} {col}.png"));
               }
           });
        }
    }
}
