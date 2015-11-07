using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Fractals.Utility;

namespace TileGluer
{
    class Program
    {
        const string ImPath = @"C:\Program Files\ImageMagick-6.9.2-Q8";

        static void Main(string[] args)
        {
            var basePath = args[0];

            var firstLevelPath = Directory.GetDirectories(basePath).Single();
            var inputZoomLevel = Int32.Parse(firstLevelPath.Split(Path.DirectorySeparatorChar).Last());

            var timer = Stopwatch.StartNew();
            while (inputZoomLevel != 0)
            {
                WL($"{DateTime.Now.ToString("s")}: Gluing zoom level {inputZoomLevel}");
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

                    WL($"Zoom Level {inputZoomLevel}: " + progress.GetEstimate((outputRow + 1d) / (inputRowCount / 2d)));
                }

                WL($"{DateTime.Now.ToString("s")}: Done with Zoom Level {inputZoomLevel}: {timer.Elapsed}");
                inputZoomLevel--;
            }
        }

        private static void WL(object text, params object[] args)
        {
            if (text == null)
            {
                Console.WriteLine("null");
            }
            else
            {
                Console.WriteLine(text.ToString(), args);
            }
        }

        private static Task GlueTiles(IEnumerable<string> fileNames, string outputTile)
        {
            var fileNamesArgument = String.Join(" ", fileNames.Select(fileName => $"\"{fileName}\""));

            return Task.Run(() =>
            {
                using (var montage = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = Path.Combine(ImPath, "montage.exe"),
                        Arguments = $"{fileNamesArgument} -tile 2x2 -geometry 128x128+0+0 \"{outputTile}\"",
                        UseShellExecute = false,
                        CreateNoWindow = true,
                    }
                })
                {
                    montage.Start();
                    montage.WaitForExit();
                }
            });
        }        
    }
}
