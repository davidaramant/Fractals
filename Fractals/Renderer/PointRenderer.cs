using Fractals.Utility;
using log4net;
using System;
using System.Drawing;
using System.IO;
using System.Linq;

namespace Fractals.Renderer
{
    public sealed class PointRenderer
    {
        private readonly string _inputInputDirectory;
        private readonly string _inputFilename;

        private readonly Size _resolution;

        private static ILog _log;

        private readonly ColorRamp _colorRamp = ColorRampFactory.Blue;

        public PointRenderer(string inputDirectory, string inputFilename, int width, int height)
        {
            _inputInputDirectory = inputDirectory;
            _inputFilename = inputFilename;

            _resolution = new Size(width, height);

            _log = LogManager.GetLogger(GetType());
        }

        public void Render(string outputDirectory, string outputFilename)
        {
            _log.InfoFormat("Creating image ({0}x{1})", _resolution.Width, _resolution.Height);

            var viewPort = AreaFactory.RenderingArea;
            viewPort.LogViewport();

            _log.Info("Loading points...");

            var listReader = new ComplexNumberListReader(_inputInputDirectory, _inputFilename);
            var points = listReader
                .GetNumbers()
                .Select(n => viewPort.GetPointFromNumber(_resolution, n))
                .Distinct()
                .ToArray();

            _log.Info("Done loading");
            _log.DebugFormat("{0} distinct points found (for the specified resolution)", points.Length);

            _log.Info("Starting to render");

            var outputImg = new Bitmap(_resolution.Width, _resolution.Height);

            foreach (var point in _resolution.GetAllPoints())
            {
                outputImg.SetPixel(point.X, point.Y, Color.Black);
            }

            foreach (var point in points)
            {
                if ((point.X < 0) || (point.Y < 0))
                {
                    continue;
                }

                outputImg.SetPixel(point.X, point.Y, Color.DeepSkyBlue);
            }

            _log.Info("Finished rendering");

            _log.Debug("Saving image");
            outputImg.Save(Path.Combine(outputDirectory, String.Format("{0}.png", outputFilename)));
            _log.Debug("Done saving image");

        }
    }
}
