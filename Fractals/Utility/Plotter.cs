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
    public class Plotter
    {
        private readonly string _directory;
        private readonly string _inputFilename;
        private readonly string _filename;
        private readonly Size _resolution;


        private static ILog _log;

        public Plotter(string directory, string inputFilename, string filename, int width, int height)
        {
            _directory = directory;
            _inputFilename = inputFilename;
            _filename = filename;
            _resolution = new Size(width, height);

            _log = LogManager.GetLogger(GetType());
        }

        public void Plot()
        {
            var list = new ComplexNumberListReader(_directory, _inputFilename);

            var viewPort = new Area(
                                        realRange: new InclusiveRange(-1.75, 1),
                                        imagRange: new InclusiveRange(-1.3, 1.3));

            var plot = new int[_resolution.Width, _resolution.Height];

            var rotatedResolution = new Size(_resolution.Height, _resolution.Width);

            Parallel.ForEach(list.GetNumbers(), number =>
            {
                foreach (var c in GetTrajectory(number))
                {
                    var point = viewPort.GetPointFromNumber(rotatedResolution, c).Rotate();

                    if (point.X < 0 || point.X >= _resolution.Width || point.Y < 0 || point.Y >= _resolution.Height)
                    {
                        continue;
                    }

                    Interlocked.Increment(ref plot[point.X, point.Y]);
                }
            });

            var max = 0;
            for (int x = 0; x < _resolution.Width; x++)
            {
                for (int y = 0; y < _resolution.Height; y++)
                {
                    var temp = plot[x, y];
                    if (temp > max)
                    {
                        max = temp;
                    }
                }
            }

            var output = new Color[_resolution.Width, _resolution.Height];

            for (int x = 0; x < _resolution.Width; x++)
            {
                for (int y = 0; y < _resolution.Height; y++)
                {
                    var current = plot[x, y];

                    var exp = Gamma(1.0 - Math.Pow(Math.E, -10.0*current/max));

                    output[x, y] = new HsvColor(
                        hue: 196.0 / 360.0,
                        saturation: (exp < 0.5) ? 1 : 1 - (2*(exp-0.5)),
                        value: ( exp < 0.5 ) ? 2*exp : 1
                    ).ToColor();
                }
            }

            ImageUtility.ColorMatrixToBitmap(output).Save(Path.Combine(_directory, String.Format("{0}.png", _filename)));
        }

        private const double DefaultGamma = 1.2;

        private static double Gamma(double x, double exp = DefaultGamma)
        {
            return Math.Pow(x, 1.0 / exp);
        }

        static IEnumerable<Complex> GetTrajectory(Complex c)
        {
            Complex z = c;

            for (int i = 0; i < 30000; i++)
            {
                z = z * z + c;

                yield return z;

                if (z.MagnitudeSquared() > 4)
                {
                    yield break;
                }
            }
        }
    }
}