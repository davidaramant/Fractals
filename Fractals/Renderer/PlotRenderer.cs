using Fractals.Arguments;
using Fractals.Utility;
using log4net;
using System;
using System.Collections.Generic;
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
            var timer = Stopwatch.StartNew();

            using (var hitPlot = new HitPlotStream(Path.Combine(_inputInputDirectory, _inputFilename), _resolution))
            {
                RenderAllTiles(hitPlot, outputDirectory);
                //ComputeHistogram(hitPlot, outputFilename);
                //RenderSomeTiles(hitPlot, outputDirectory);
            }

            timer.Stop();

            _log.Info($"Finished. Took: {timer.Elapsed}");
        }

        private static IEnumerable<Point> GetAllTileIndexes(int rows, int cols)
        {
            for (var row = 0; row < rows; row++)
            {
                for (var col = 0; col < cols; col++)
                {
                    yield return new Point(col, row);
                }
            }
        }

        private void RenderAllTiles(HitPlotStream hitPlot, string outputDirectory)
        {
            var numberOfTiles = (_resolution.Width / TileSize) * (_resolution.Height / TileSize);
            _log.Info($"Creating image ({_resolution.Width:N0}x{_resolution.Height:N0}) ({numberOfTiles:N0} tiles)");

            _log.Info("Starting to render");

            var rows = _resolution.Height / TileSize;
            var cols = _resolution.Width / TileSize;

            for (int rowIndex = 0; rowIndex < rows; rowIndex++)
            {
                var rowDir = Path.Combine(outputDirectory, rowIndex.ToString());
                Directory.CreateDirectory(rowDir);
            }

            RenderTileIndexes(numberOfTiles, GetAllTileIndexes(rows, cols), cols, hitPlot, outputDirectory);
        }

        private void RenderSomeTiles(HitPlotStream hitPlot, string outputDirectory)
        {
            // X,Y
            var tilesToRender = new[]
            {
                new Point(832,268),
                new Point(833,268),
            };

            var numberOfTiles = tilesToRender.Length;
            _log.Info($"Creating image ({_resolution.Width:N0}x{_resolution.Height:N0}) ({numberOfTiles:N0} tiles)");

            _log.Info("Starting to render");

            var cols = _resolution.Width / TileSize;

            foreach (var rowIndex in tilesToRender.Select(p => p.Y).Distinct())
            {
                var rowDir = Path.Combine(outputDirectory, rowIndex.ToString());
                Directory.CreateDirectory(rowDir);
            }

            // HACK: Tiles must be continuous
            hitPlot.SetStreamOffset(cols * tilesToRender.First().Y + tilesToRender.First().X);

            RenderTileIndexes(numberOfTiles, tilesToRender, cols, hitPlot, outputDirectory);
        }

        private void RenderTileIndexes(int numberOfTiles, IEnumerable<Point> tileIndexes, int cols, HitPlotStream hitPlot, string outputDirectory)
        {
            var whenToCheck = Math.Max(1, numberOfTiles / 256);
            var progress = ProgressEstimator.Start();

            Task.WhenAll(
                    tileIndexes.
                    Select((tileId, currentTileIndex) =>
                    {
                        return
                            hitPlot.ReadTileBufferAsync().
                            ContinueWith(byteBufferTask =>
                            {
                                var byteBuffer = byteBufferTask.Result;

                                using (var imageTile = new FastImage(TileSize))
                                {
                                    for (int i = 0; i < byteBuffer.Length; i += 2)
                                    {
                                        var current = BitConverter.ToUInt16(byteBuffer, i);
                                        imageTile.SetPixel(i / 2, ColorGradients.ColorCount(current));

                                    }

                                    // HACK: Dump tile data
                                    //File.WriteAllBytes(Path.Combine(outputDirectory, tileId.Y.ToString(), tileId.X + ".data"), byteBuffer);

                                    imageTile.Save(Path.Combine(outputDirectory, tileId.Y.ToString(), tileId.X + ".jpg"));
                                }

                                if (currentTileIndex % whenToCheck == 0)
                                {
                                    if (currentTileIndex != 0)
                                    {
                                        _log.Info(progress.GetEstimate((double)currentTileIndex / numberOfTiles));
                                    }
                                }
                            });
                    })).Wait();
        }

        private void ComputeHistogram(HitPlotStream hitPlot, string outputFileName)
        {
            _log.Info($"Computing histograms...");

            var completeHistrogram = new Histogram();
            var cappedHistogram = new CappedHistogram(5000);

            ulong total = (ulong)_resolution.Width * (ulong)_resolution.Height;
            ulong whenToCheck = total / 256;
            ulong pointsProcessed = 0;

            var progress = ProgressEstimator.Start();

            foreach (var count in hitPlot.GetAllCounts())
            {
                completeHistrogram.IncrementBin(count);
                cappedHistogram.IncrementBin(count);
                pointsProcessed++;

                if (pointsProcessed % whenToCheck == 0)
                {
                    var percentageComplete = (double)pointsProcessed / (double)total;
                    _log.Info(progress.GetEstimate(percentageComplete));
                }
            }

            completeHistrogram.SaveToCsv($"{DateTime.Now.ToString("yyyyMMddHHmm")} - Complete Histogram.csv");
            cappedHistogram.SaveToCsv($"{DateTime.Now.ToString("yyyyMMddHHmm")} - Capped Histrogram.csv");

            _log.Info("Done computing histograms!");
        }
    }
}
