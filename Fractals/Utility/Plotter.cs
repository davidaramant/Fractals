using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Fractals.Model;
using log4net;

namespace Fractals.Utility
{
    public sealed class Plotter
    {
        private readonly string _inputDirectory;
        private readonly string _inputFilenamePattern;
        private readonly string _directory;
        private readonly string _filename;
        private readonly Size _resolution;

        #region Hit arrays

        private int _fourthWidth;
        private int _fourthHeight;

        private int[] _hits00;
        private int[] _hits10;
        private int[] _hits20;
        private int[] _hits30;

        private int[] _hits01;
        private int[] _hits11;
        private int[] _hits21;
        private int[] _hits31;

        private int[] _hits02;
        private int[] _hits12;
        private int[] _hits22;
        private int[] _hits32;

        private int[] _hits03;
        private int[] _hits13;
        private int[] _hits23;
        private int[] _hits33;

        #endregion Hit arrays

        private static ILog _log;

        public Plotter(string inputDirectory, string inputFilenamePattern, string directory, string filename, int width, int height)
        {
            _inputDirectory = inputDirectory;
            _inputFilenamePattern = inputFilenamePattern;
            _directory = directory;
            _filename = filename;
            _resolution = new Size(width, height);

            _log = LogManager.GetLogger(GetType());

            if (width % 16 != 0)
            {
                _log.Warn("The width should be evenly divisible by 16");
            }
            if (height % 16 != 0)
            {
                _log.Warn("The height should be evenly divisible by 16");
            }

            InitializeHitPlot();
        }

        #region Hit Plot Array Operations

        private void InitializeHitPlot()
        {
            _fourthWidth = _resolution.Width / 4;
            _fourthHeight = _resolution.Height / 4;

            int quadrantSize = _fourthWidth * _fourthHeight;

            _hits00 = new int[quadrantSize];
            _hits10 = new int[quadrantSize];
            _hits20 = new int[quadrantSize];
            _hits30 = new int[quadrantSize];

            _hits01 = new int[quadrantSize];
            _hits11 = new int[quadrantSize];
            _hits21 = new int[quadrantSize];
            _hits31 = new int[quadrantSize];

            _hits02 = new int[quadrantSize];
            _hits12 = new int[quadrantSize];
            _hits22 = new int[quadrantSize];
            _hits32 = new int[quadrantSize];

            _hits03 = new int[quadrantSize];
            _hits13 = new int[quadrantSize];
            _hits23 = new int[quadrantSize];
            _hits33 = new int[quadrantSize];
        }

        private int[] GetSegment(int x, int y)
        {
            var xQuadrant = x / _fourthWidth;
            var yQuadrant = y / _fourthHeight;

            // Handle points that fall exactly on the edge
            if (xQuadrant == 4)
                xQuadrant--;
            if (yQuadrant == 4)
                yQuadrant--;

            switch (xQuadrant)
            {
                case 0:
                    switch (yQuadrant)
                    {
                        case 0:
                            return _hits00;
                        case 1:
                            return _hits01;
                        case 2:
                            return _hits02;
                        case 3:
                            return _hits03;

                        default:
                            throw new Exception("NO WAY");
                    }

                case 1:
                    switch (yQuadrant)
                    {
                        case 0:
                            return _hits10;
                        case 1:
                            return _hits11;
                        case 2:
                            return _hits12;
                        case 3:
                            return _hits13;

                        default:
                            throw new Exception("NO WAY");
                    }

                case 2:
                    switch (yQuadrant)
                    {
                        case 0:
                            return _hits20;
                        case 1:
                            return _hits21;
                        case 2:
                            return _hits22;
                        case 3:
                            return _hits23;

                        default:
                            throw new Exception("NO WAY");
                    }

                case 3:
                    switch (yQuadrant)
                    {
                        case 0:
                            return _hits30;
                        case 1:
                            return _hits31;
                        case 2:
                            return _hits32;
                        case 3:
                            return _hits33;

                        default:
                            throw new Exception("NO WAY");
                    }

                default:
                    throw new Exception("NO WAY");
            }
        }

        private void IncrementPoint(Point p)
        {
            var segment = GetSegment(p.X, p.Y);

            var offset = (p.X % _fourthWidth) + (_fourthWidth * (p.Y % _fourthHeight));

            Interlocked.Increment(ref segment[offset]);
        }

        private int GetHitsForPoint(Point p)
        {
            var segment = GetSegment(p.X, p.Y);

            var offset = (p.X % _fourthWidth) + (_fourthWidth * (p.Y % _fourthHeight));

            return segment[offset];
        }

        private int FindMaximumHit()
        {
            return new[]
            {
                _hits00.Max(),
                _hits10.Max(),
                _hits20.Max(),
                _hits30.Max(),

                _hits01.Max(),
                _hits11.Max(),
                _hits21.Max(),
                _hits31.Max(),

                _hits02.Max(),
                _hits12.Max(),
                _hits22.Max(),
                _hits32.Max(),

                _hits03.Max(),
                _hits13.Max(),
                _hits23.Max(),
                _hits33.Max(),

            }.Max();
        }

        #endregion Hit Plot Array Operations

        public void Plot()
        {
            _log.InfoFormat("Plotting image ({0}x{1})", _resolution.Width, _resolution.Height);

            var list = new ComplexNumberListReader(_inputDirectory, _inputFilenamePattern);

            var viewPort = new Area(
                            realRange: new InclusiveRange(-1.75, 1),
                            imaginaryRange: new InclusiveRange(-1.3, 1.3));

            viewPort.LogViewport();

            var rotatedResolution = new Size(_resolution.Height, _resolution.Width);

            _log.Debug("Calculating trajectories");

            Parallel.ForEach(list.GetNumbers(), number =>
            {
                foreach (var c in GetTrajectory(number))
                {
                    var point = viewPort.GetPointFromNumber(rotatedResolution, c).Rotate();

                    if (!_resolution.IsInside(point))
                    {
                        continue;
                    }

                    IncrementPoint(point);
                }
            });

            _log.Debug("Done plotting trajectories");

            var max = FindMaximumHit();

            _log.DebugFormat("Found maximum: {0}", max);

            var outputImg = new Bitmap(_resolution.Width, _resolution.Height);

            var processedPixels =
                _resolution.GetAllPoints().
                AsParallel().
                Select(p => ComputeColor(p, max)).
                AsEnumerable();

            foreach (var result in processedPixels)
            {
                outputImg.SetPixel(result.Item1.X, result.Item1.Y, result.Item2);
            }

            _log.Debug("Done setting pixels");

            _log.Debug("Saving image");
            outputImg.Save(Path.Combine(_directory, String.Format("{0}.png", _filename)));
            _log.Debug("Done saving image");
        }

        private Tuple<Point, Color> ComputeColor(Point p, int max)
        {
            var current = GetHitsForPoint(p);

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

        private const int Bailout = 30000;

        static IEnumerable<Complex> GetTrajectory(Complex c)
        {
            var rePrev = c.Real;
            var imPrev = c.Imaginary;

            double re = 0;
            double im = 0;

            for (int i = 0; i < Bailout; i++)
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