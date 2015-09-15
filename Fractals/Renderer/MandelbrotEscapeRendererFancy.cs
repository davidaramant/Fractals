using System;
using System.Drawing;
using System.Linq;
using Fractals.Arguments;
using Fractals.Model;
using Fractals.Utility;
using log4net;

namespace Fractals.Renderer
{
    public sealed class MandelbrotEscapeRendererFancy : IGenerator
    {
        private static ILog _log;

        public MandelbrotEscapeRendererFancy()
        {
            _log = LogManager.GetLogger(GetType());
        }

        public Color[,] Render(Size resolution, Area viewPort)
        {
            _log.InfoFormat("Starting to render ({0:N0}x{1:N0})", resolution.Width, resolution.Height);

            viewPort.LogViewport();

            var output = new Color[resolution.Width, resolution.Height];

            _log.Debug("Rendering points");

            var allPointsWithEscapeTimes =
                resolution
                .GetAllPoints()
                .AsParallel()
                .WithDegreeOfParallelism(GlobalArguments.DegreesOfParallelism)
                .Select(p => Tuple.Create(p, PickColor(FindEscapeTime(viewPort.GetNumberFromPoint(resolution, p)))))
                .AsEnumerable();

            foreach (var result in allPointsWithEscapeTimes)
            {
                output[result.Item1.X, result.Item1.Y] = result.Item2;
            }

            return output;
        }

        private static double Gamma(double x, double exp = 1.2)
        {
            return Math.Pow(x, 1.0 / exp);
        }

        private static Color PickColor(int escapeTime)
        {
            if (escapeTime == -1)
            {
                return Color.Black;
            }

            var exp = Gamma(1.0 - Math.Pow(Math.E, -40.0 * escapeTime / Bailout));

            return new HsvColor(
                hue:0.85,
                saturation: (exp < 0.5) ? 1 : 1 - (2 * (exp - 0.5)),
                value: (exp < 0.5) ? 2 * exp : 1
            ).ToColor();
        }

        private const int Bailout = 3000;

        public static int FindEscapeTime(Complex c)
        {
            if (MandelbrotFinder.IsInSet(c))
            {
                return -1;
            }

            var rePrev = c.Real;
            var imPrev = c.Imaginary;

            double re = 0;
            double im = 0;

            for (int i = 0; i < Bailout; i++)
            {
                var reTemp = re * re - im * im + rePrev;
                im = 2 * re * im + imPrev;
                re = reTemp;

                var magnitudeSquared = re * re + im * im;
                if (magnitudeSquared > 4)
                {
                    return i;
                }
            }

            return -1;
        }
    }
}
