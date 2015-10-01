using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace TileGluer
{
    class Program
    {
        static void Main(string[] args)
        {
            var basePath = args[0];
            var imPath = @"C:\Program Files\ImageMagick-6.9.2-Q16";

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

                Parallel.For(0, inputRowCount / 2, outputRow =>
                {
                    var outputRowPath = Path.Combine(outputZoomPath, outputRow.ToString());
                    Directory.CreateDirectory(outputRowPath);
                    for (int outputCol = 0; outputCol < inputRowCount / 2; outputCol++)
                    {
                        var inputTiles =
                            Enumerable.Range(outputRow * 2, 2)
                                .SelectMany(
                                    startRow =>
                                        Enumerable.Range(outputCol * 2, 2)
                                            .Select(
                                                startCol =>
                                                    Path.Combine(basePath, inputZoomLevel.ToString(),
                                                        startRow.ToString(),
                                                        startCol + ".png"))).ToArray();

                        var outputTile = Path.Combine(outputRowPath, outputCol + ".png");

                        var montage = new Process
                        {
                            StartInfo = new ProcessStartInfo
                            {
                                FileName = Path.Combine(imPath, "montage.exe"),
                                Arguments =
                                    $"{String.Join(" ", inputTiles.Select(fileName => $"\"{fileName}\""))} -geometry 200x200+0+0 \"{outputTile}\"",
                                UseShellExecute = false,
                                CreateNoWindow = true,
                            }
                        };
                        montage.Start();
                        montage.WaitForExit();

                        var convert = new Process
                        {
                            StartInfo = new ProcessStartInfo
                            {
                                FileName = Path.Combine(imPath, "convert.exe"),
                                Arguments = $"\"{outputTile}\" -resize 200x200+0+0 \"{outputTile}\"",
                                UseShellExecute = false,
                                CreateNoWindow = true,
                            }
                        };
                        convert.Start();
                        convert.WaitForExit();
                    }
                });

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
    }
}
