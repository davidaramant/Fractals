using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Fractals.Utility;
using static System.Console;

namespace TileGluer
{
    class Program
    {
        const string ImPath = @"C:\Program Files\ImageMagick-6.9.2-Q8";

        struct TileCoordinate
        {
            public readonly int ZoomLevel;
            public readonly int Y;
            public readonly int X;

            public TileCoordinate(int zoomLevel, int x, int y)
            {
                ZoomLevel = zoomLevel;
                Y = y;
                X = x;
            }

            public string RelativePath => Path.Combine(ZoomLevel.ToString(), Y.ToString(), X + ".png");

            public override string ToString() => $"Z: {ZoomLevel}, Y: {Y}, X: {X}";
        }

        static void Main(string[] args)
        {
            switch (args.Length)
            {
                case 1:
                    CreateAllZoomLevels(basePath: args[0]);
                    break;
                case 2:
                    FixZoomLevels(basePath: args[0], inputFilePath: args[1]);
                    break;
                default:
                    throw new NotImplementedException("Unsupported arguments");
            }
        }

        private static void CreateAllZoomLevels(string basePath)
        {
            var firstLevelPath = Directory.GetDirectories(basePath).Single();
            var inputZoomLevel = Int32.Parse(firstLevelPath.Split(Path.DirectorySeparatorChar).Last());

            var timer = Stopwatch.StartNew();
            while (inputZoomLevel != 0)
            {
                WriteLine($"{DateTime.Now.ToString("s")}: Gluing zoom level {inputZoomLevel}");
                var inputRowCount = 1 << inputZoomLevel;
                var outputZoomLevel = inputZoomLevel - 1;

                var outputZoomPath = Path.Combine(basePath, outputZoomLevel.ToString());
                Directory.CreateDirectory(outputZoomPath);

                var progress = ProgressEstimator.Start();
                for (int outputRow = 0; outputRow < inputRowCount / 2; outputRow++)
                {
                    var outputRowPath = Path.Combine(outputZoomPath, outputRow.ToString());
                    Directory.CreateDirectory(outputRowPath);

                    var outRow = outputRow;
                    var inputZoom = inputZoomLevel;
                    Task.WhenAll(
                        Enumerable.Range(0, inputRowCount / 2).Select(outputCol =>
                          {
                              var inputTiles =
                                  Enumerable.Range(outRow * 2, 2)
                                      .SelectMany(
                                          startRow =>
                                              Enumerable.Range(outputCol * 2, 2)
                                                  .Select(
                                                      startCol =>
                                                          Path.Combine(basePath, inputZoom.ToString(),
                                                              startRow.ToString(),
                                                              startCol + ".png"))).ToArray();

                              var outputTile = Path.Combine(outputRowPath, outputCol + ".png");

                              return GlueTiles(inputTiles, outputTile);
                          })).Wait();

                    WriteLine($"Zoom Level {inputZoomLevel}: " + progress.GetEstimate((outputRow + 1d) / (inputRowCount / 2d)));
                }

                WriteLine($"{DateTime.Now.ToString("s")}: Done with Zoom Level {inputZoomLevel}: {timer.Elapsed}");
                inputZoomLevel--;
            }
        }

        private static void FixZoomLevels(string basePath, string inputFilePath)
        {
            var allTiles = ParseInputFile(inputFilePath);

            foreach (var tile in allTiles)
            {
                WriteLine(tile);
            }

            var outputZoomLevel = allTiles.Max(tile => tile.ZoomLevel);
            while (outputZoomLevel != 0)
            {
                var tilesAtThisLevel = allTiles.Where(tile => tile.ZoomLevel == outputZoomLevel).ToArray();

                WriteLine($"Gluing in zoom level: {outputZoomLevel}");
                foreach (var tile in tilesAtThisLevel)
                {
                    WriteLine(tile);
                }

                Task.WhenAll(tilesAtThisLevel.Select(tiles => ReglueTile(basePath, tiles))).Wait();

                foreach (var tile in tilesAtThisLevel)
                {
                    allTiles.Add(new TileCoordinate(zoomLevel: outputZoomLevel - 1, x: tile.X / 2, y: tile.Y / 2));
                }

                outputZoomLevel--;
            }
        }

        private static HashSet<TileCoordinate> ParseInputFile(string inputFilePath)
        {
            return
                new HashSet<TileCoordinate>(
                    File.ReadLines(inputFilePath)
                    .Where(line => !line.StartsWith("#"))
                    .Select(line => line.Split('/'))
                    .Select(partArray => partArray.Skip(partArray.Length - 3).ToArray())
                    .Select(parts =>

                        new TileCoordinate(
                            zoomLevel: Int32.Parse(parts[0]),
                            y: Int32.Parse(parts[1]),
                            x: Int32.Parse(parts[2].Replace(".png", ""))
                    )));
        }

        private static Task ReglueTile(string basePath, TileCoordinate tile)
        {
            var parentTiles = new[]
            {
                new TileCoordinate(tile.ZoomLevel+1,tile.X*2,tile.Y*2),
                new TileCoordinate(tile.ZoomLevel+1,tile.X*2+1,tile.Y*2),
                new TileCoordinate(tile.ZoomLevel+1,tile.X*2,tile.Y*2+1),
                new TileCoordinate(tile.ZoomLevel+1,tile.X*2+1,tile.Y*2+1),
            };

            var inputFileNames =
                parentTiles.Select(position => Path.Combine(basePath, position.RelativePath));

            return GlueTiles(inputFileNames, Path.Combine(basePath, tile.RelativePath));
        }

        private static Task GlueTiles(IEnumerable<string> fileNames, string outputTile)
        {
            var fileNamesArgument = String.Join(" ", fileNames.Select(fileName => $"\"{fileName}\""));
            var arguments = $"{fileNamesArgument} -tile 2x2 -geometry 128x128+0+0 \"{outputTile}\"";

            // there is no non-generic TaskCompletionSource
            var tcs = new TaskCompletionSource<bool>();

            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = Path.Combine(ImPath, "montage.exe"),
                    Arguments = arguments,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                },
                EnableRaisingEvents = true
            };

            process.Exited += (sender, args) =>
            {
                tcs.SetResult(true);
                process.Dispose();
            };

            process.Start();

            return tcs.Task;
        }
    }
}
