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

        public PointRenderer(string inputDirectory, string inputFilename, int width, int height)
        {
            _inputInputDirectory = inputDirectory;
            _inputFilename = inputFilename;

            _resolution = new Size(width, height);

            _log = LogManager.GetLogger(GetType());
        }

        public void Render(string outputDirectory, string outputFilename)
        {
            Render(outputDirectory, outputFilename, ColorRampFactory.Rainbow);
        }

        public void Render(string outputDirectory, string outputFilename, ColorRamp colorRamp)
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

            var middlePoint = new Point(_resolution.Width / 2, _resolution.Height / 2);
            var maximumDistance = CalculateDistance(middlePoint, new Point(0, 0));

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

                outputImg.SetPixel(point.X, point.Y, ComputeColor(point, middlePoint, maximumDistance, colorRamp));
            }

            _log.Info("Finished rendering");

            _log.Debug("Saving image");
            outputImg.Save(Path.Combine(outputDirectory, String.Format("{0}.png", outputFilename)));
            _log.Debug("Done saving image");
        }

        private Color ComputeColor(Point p, Point middlePoint, double maximumDistance, ColorRamp colorRamp)
        {
            var distance = CalculateDistance(p, middlePoint);
            var ratio = Gamma(1.0 - Math.Pow(Math.E, -10.0 * distance / maximumDistance));

            return colorRamp.GetColor(ratio).ToColor();
        }

        private static double CalculateDistance(Point point1, Point point2)
        {
            return (Math.Pow(point1.X - point2.X, 2) + Math.Pow(point1.Y - point2.Y, 2));
        }

        private static double Gamma(double x, double exp = 1.2)
        {
            return Math.Pow(x, 1.0 / exp);
        }
    }
}
