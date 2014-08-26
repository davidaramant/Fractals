using Fractals.Model;
using Fractals.Utility;
using log4net;
using System.Drawing;

namespace Fractals.Renderer
{
    public sealed class MandelbrotEscapeRenderer : IGenerator
    {
        private static ILog _log;

        public MandelbrotEscapeRenderer()
        {
            _log = LogManager.GetLogger(GetType());
        }

        public Color[,] Render(Size resolution, Area viewPort)
        {
            _log.InfoFormat("Starting to render ({0:N0}x{1:N0})", resolution.Width, resolution.Height);

            viewPort = AreaFactory.MandelbrotRadiusArea;

            viewPort.LogViewport();

            var output = new Color[resolution.Width, resolution.Height];

            _log.Debug("Rendering points");
            for (int y = 0; y < resolution.Height; y++)
            {
                for (int x = 0; x < resolution.Width; x++)
                {
                    var number = viewPort.GetNumberFromPoint(resolution, new Point(x, y));
                    Color color = PickColor(IsInSet(number));
                    output[x, y] = color;
                }
            }

            return output;
        }

        private static Color PickColor(int escapeTime)
        {
            return escapeTime % 2 == 0 ? Color.Black : Color.White;
        }

        private const int Bailout = 3000;

        public static int IsInSet(Complex c)
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
