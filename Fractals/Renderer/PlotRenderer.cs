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
            //Render( outputDirectory, outputFilename, ColorGradients.Blue );
            //Render(outputDirectory, outputFilename, ColorGradients.Psychadelic);
            Render(outputDirectory, outputFilename, ColorGradients.Neon);
        }

        public void Render(string outputDirectory, string outputFilename, ColorGradient colorGradient)
        {
            var timer = Stopwatch.StartNew();

            using (var hitPlot = new HitPlotStream(Path.Combine(_inputInputDirectory, _inputFilename), _resolution))
            {
                //RenderAllTiles(hitPlot, ColorGradient, outputDirectory);
                ComputeHistogram(hitPlot, outputFilename);
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

        private static double Gamma(double x, double exp = 1.2)
        {
            return Math.Pow(x, 1.0 / exp);
        }

        private void RenderAllTiles(HitPlotStream hitPlot, ColorGradient gradient, string outputDirectory)
        {
            var numberOfTiles = (_resolution.Width / TileSize) * (_resolution.Height / TileSize);
            _log.Info($"Creating image ({_resolution.Width:N0}x{_resolution.Height:N0}) ({numberOfTiles:N0} tiles)");

            const ushort cappedMax = 5000;

            _log.Debug($"Using maximum: {cappedMax:N0}");

            _log.Info("Starting to render");

            var rows = _resolution.Height / TileSize;
            ;
            var cols = _resolution.Width / TileSize;

            for (int rowIndex = 0; rowIndex < rows; rowIndex++)
            {
                var rowDir = Path.Combine(outputDirectory, rowIndex.ToString());
                Directory.CreateDirectory(rowDir);
            }

            var totalNumberOfTiles = rows * cols;
            var whenToCheck = totalNumberOfTiles / 256;
            var progress = ProgressEstimator.Start();

            Task.WhenAll(
                    GetAllTileIndexes(rows: rows, cols: cols).
                    Select((tileId, currentTileIndex) =>
                    {
                        return
                            hitPlot.ReadTileBufferAsync(cols * tileId.Y + tileId.X).
                            ContinueWith(byteBufferTask =>
                            {
                                var byteBuffer = byteBufferTask.Result;

                                using (var imageTile = new FastBitmap(TileSize))
                                {
                                    for (int i = 0; i < byteBuffer.Length; i += 2)
                                    {
                                        var current = BitConverter.ToUInt16(byteBuffer, i);
                                        current = Math.Min(current, cappedMax);
                                        var ratio = Gamma(1.0 - Math.Pow(Math.E, -15.0 * current / cappedMax));
                                        imageTile.SetPixel(i / 2, gradient.GetColor(ratio));
                                    }
                                    imageTile.Save(Path.Combine(outputDirectory, tileId.Y.ToString(), tileId.X + ".png"));
                                }

                                if (currentTileIndex % whenToCheck == 0)
                                {
                                    if (currentTileIndex != 0)
                                    {
                                        _log.Info(progress.GetEstimate((double)currentTileIndex / totalNumberOfTiles));
                                    }
                                }
                            });
                    })).Wait();

        }

        private void ComputeHistogram(HitPlotStream hitPlot, string outputFileName)
        {
            _log.Info($"Computing histogram...");

            var totalHistogram = new Histogram();

            ulong total = (ulong)_resolution.Width * (ulong)_resolution.Height;
            ulong whenToCheck = total / 256;
            ulong pointsProcessed = 0;

            var progress = ProgressEstimator.Start();

            foreach (var count in hitPlot.GetAllCounts())
            {
                totalHistogram.IncrementBin(count);
                pointsProcessed++;

                if (pointsProcessed % whenToCheck == 0)
                {
                    var percentageComplete = (double)pointsProcessed / (double)total;
                    _log.Info(progress.GetEstimate(percentageComplete));
                }
            }

            totalHistogram.SaveToCsv($"{outputFileName} - {DateTime.Now.ToString("yyyyMMddHHmm")}.csv");

            _log.Info("Done getting histogram!");
        }

        private void RenderSomeTiles(HitPlotReader hitPlotWriter, ushort cappedMax, ColorGradient gradient, string outputDirectory)
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

               using (var tile = new FastBitmap(TileSize))
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

                           var color = gradient.GetColor(ratio);
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
