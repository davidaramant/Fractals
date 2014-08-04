using System;
using System.Drawing;
using System.Linq;
using Fractals.Arguments;
using Fractals.Model;
using Fractals.Utility;
using log4net;

namespace Fractals.Renderer
{
    public sealed class MandelbrotDistanceRenderer : IGenerator
    {
        private static ILog _log;

        public MandelbrotDistanceRenderer()
        {
            _log = LogManager.GetLogger(GetType());
        }

        public Color[,] Render(Size resolution, InclusiveRange realAxis, InclusiveRange imaginaryAxis)
        {
            _log.InfoFormat("Starting to render ({0}x{1})", resolution.Width, resolution.Height);

            var viewPort = new Area(
                realRange: new InclusiveRange(-2, 1),
                imaginaryRange: new InclusiveRange(-1.5, 1.5));

            viewPort.LogViewport();

            var output = new Color[resolution.Width, resolution.Height];

            _log.Debug("Rendering points");

            var allPointsWithEscapeTimesAndDistance =
                resolution
                    .GetAllPoints()
                    .AsParallel()
                    .WithDegreeOfParallelism(GlobalArguments.DegreesOfParallelism)
                    .Select(p => Tuple.Create(p, FindEscapeTimeAndDistance(viewPort.GetNumberFromPoint(resolution, p))))
                    .AsEnumerable()
                    .ToArray();

            var maxDistance = allPointsWithEscapeTimesAndDistance.Select(_ => _.Item2).Select(_ => _.Item2).Max();

            foreach (var result in allPointsWithEscapeTimesAndDistance)
            {
                output[result.Item1.X, result.Item1.Y] = PickColor(result.Item2, maxDistance);
            }

            return output;
        }

        private static double Gamma(double x, double exp = 1.2)
        {
            return Math.Pow(x, 1.0 / exp);
        }

        private static Color PickColor(Tuple<int, double, double> results, double maxDistance)
        {
            var escapeTime = results.Item1;
            var distance = results.Item2;
            var angle = results.Item3;

            var distanceGradient = Gamma(1.0 - Math.Pow(Math.E, -10.0 * distance / maxDistance));
            var escapeTimeGradient = 
                escapeTime == -1 ?
                1.0 :
                Gamma(1.0 - Math.Pow(Math.E, -10.0 * escapeTime / Bailout));

            return new HsvColor(
                hue: 0.2 * (angle + Math.PI) / (2*Math.PI),
                saturation: 1,
                value: 1 - distanceGradient
            ).ToColor();
        }

        private const int Bailout = 5000;

        public static Tuple<int, double, double> FindEscapeTimeAndDistance(Complex c)
        {
            var rePrev = c.Real;
            var imPrev = c.Imaginary;

            double re = 0;
            double im = 0;

            double minDistance = Double.MaxValue;
            double angle = 0;

            for (int i = 0; i < Bailout; i++)
            {
                var reTemp = re * re - im * im + rePrev;
                im = 2 * re * im + imPrev;
                re = reTemp;

                var oldMindistance = minDistance;

                minDistance = Math.Min(minDistance, Math.Abs(re));
                minDistance = Math.Min(minDistance, Math.Abs(im));

                if (minDistance < oldMindistance)
                {
                    angle = Math.Atan2(im, re);
                }

                var magnitudeSquared = re * re + im * im;

                if (magnitudeSquared > 100)
                {
                    return Tuple.Create(i, minDistance, angle);
                }
            }

            return Tuple.Create(-1, minDistance, angle);
        }
    }
}
