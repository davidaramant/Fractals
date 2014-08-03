using Fractals.Model;
using log4net;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Fractals.Utility
{
    public abstract class Plotter
    {
        private readonly string _directory;
        private readonly string _filename;
        private readonly int _bailout;

        protected readonly Size Resolution;

        private readonly HitPlot _hitPlot;

        private static ILog _log;

        protected Plotter(string directory, string filename, int width, int height, int bailout)
        {
            _directory = directory;
            _filename = filename;
            Resolution = new Size(width, height);
            _bailout = bailout;

            _log = LogManager.GetLogger(GetType());

            if (width % 16 != 0)
            {
                _log.Warn("The width should be evenly divisible by 16");
            }
            if (height % 16 != 0)
            {
                _log.Warn("The height should be evenly divisible by 16");
            }

            _hitPlot = new HitPlot(Resolution);
        }

        protected abstract IEnumerable<Complex> GetNumbers();

        public void Plot()
        {
            _log.InfoFormat("Plotting image ({0}x{1})", Resolution.Width, Resolution.Height);

            var viewPort = new Area(
                            realRange: new InclusiveRange(-1.75, 1),
                            imaginaryRange: new InclusiveRange(-1.3, 1.3));

            viewPort.LogViewport();

            var rotatedResolution = new Size(Resolution.Height, Resolution.Width);

            _log.Info("Calculating trajectories");

            Parallel.ForEach(GetNumbers(), number =>
            {
                foreach (var c in GetTrajectory(number))
                {
                    var point = viewPort.GetPointFromNumber(rotatedResolution, c).Rotate();

                    if (!Resolution.IsInside(point))
                    {
                        continue;
                    }

                    _hitPlot.IncrementPoint(point);
                }
            });

            _log.Info("Done plotting trajectories");

            var max = _hitPlot.FindMaximumHit();

            _log.DebugFormat("Found maximum: {0}", max);

            _log.Info("Starting to render");

            var outputImg = new Bitmap(Resolution.Width, Resolution.Height);

            var processedPixels =
                Resolution.GetAllPoints().
                AsParallel().
                Select(p => ComputeColor(p, max)).
                AsEnumerable();

            foreach (var result in processedPixels)
            {
                outputImg.SetPixel(result.Item1.X, result.Item1.Y, result.Item2);
            }

            _log.Info("Finished rendering");

            _log.Debug("Saving image");
            outputImg.Save(Path.Combine(_directory, String.Format("{0}.png", _filename)));
            _log.Debug("Done saving image");
        }

        private Tuple<Point, Color> ComputeColor(Point p, int max)
        {
            var current = _hitPlot.GetHitsForPoint(p);

            var exp = Gamma(1.0 - Math.Pow(Math.E, -10.0 * current / max));

            return Tuple.Create(p,
                new HsvColor(
                    hue: 196.0 / 360.0,
                    saturation: (exp < 0.5) ? 1 : 1 - (2 * (exp - 0.5)),
                    value: (exp < 0.5) ? 2 * exp : 1
                ).ToColor());
        }

        private static double Gamma(double x, double exp = 1.2)
        {
            return Math.Pow(x, 1.0 / exp);
        }
        
        private IEnumerable<Complex> GetTrajectory(Complex c)
        {
            var rePrev = c.Real;
            var imPrev = c.Imaginary;

            double re = 0;
            double im = 0;

            for (int i = 0; i < _bailout; i++)
            {
                var reTemp = re * re - im * im + rePrev;
                im = 2 * re * im + imPrev;
                re = reTemp;

                yield return new Complex(re, im);

                var magnitudeSquared = re * re + im * im;
                if (magnitudeSquared > 4)
                {
                    yield break;
                }
            }
        }
    }
}