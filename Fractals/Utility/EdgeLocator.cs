using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using Fractals.Model;
using log4net;

namespace Fractals.Utility
{
    public sealed class EdgeLocator
    {
        private readonly string _outputDirectory;
        private readonly string _outputFilename;

        private readonly static ILog _log;

        static EdgeLocator()
        {
            _log = LogManager.GetLogger(typeof(EdgeLocator));
        }

        public EdgeLocator(string outputDirectory, string outputFilename)
        {
            _outputDirectory = outputDirectory;
            _outputFilename = outputFilename;
        }

        public void StoreEdges(Size resolution, double gridSize, Area viewPort)
        {
            var writer = new AreaListWriter(_outputDirectory, _outputFilename);
            writer.Truncate();

            ulong count = 0;
            foreach (var area in LocateEdges(resolution, viewPort))
            {
                count++;
                writer.SaveArea(area);
            }
            _log.Info($"Found {count:N0} total areas");
        }

        private static IEnumerable<Area> LocateEdges(Size resolution, Area viewPort)
        {
            _log.DebugFormat("Looking for intersting areas ({0:N0}x{1:N0})", resolution.Width, resolution.Height);

            double realIncrement = viewPort.RealRange.Magnitude / resolution.Width;
            double imagIncrement = viewPort.ImagRange.Magnitude / resolution.Height;

            return
                GetAllPoints(resolution).
                    AsParallel().
                    Select(point => new Area(
                        new InclusiveRange(viewPort.RealRange.Minimum + point.X * realIncrement, viewPort.RealRange.Minimum + (point.X + 1) * realIncrement),
                        new InclusiveRange(viewPort.ImagRange.Minimum + point.Y * imagIncrement, viewPort.ImagRange.Minimum + (point.Y + 1) * imagIncrement))).
                    Where(searchArea =>
                    {
                        var isLastCornerIn = MandelbrotFinder.IsInSet(new Complex(searchArea.RealRange.Minimum, searchArea.ImagRange.Minimum));
                        var isCornerIn = MandelbrotFinder.IsInSet(new Complex(searchArea.RealRange.Maximum, searchArea.ImagRange.Minimum));

                        if (isCornerIn != isLastCornerIn)
                        {
                            return true;
                        }
                        isLastCornerIn = isCornerIn;

                        isCornerIn = MandelbrotFinder.IsInSet(new Complex(searchArea.RealRange.Minimum, searchArea.ImagRange.Maximum));
                        if (isCornerIn != isLastCornerIn)
                        {
                            return true;
                        }
                        isLastCornerIn = isCornerIn;

                        isCornerIn = MandelbrotFinder.IsInSet(new Complex(searchArea.RealRange.Maximum, searchArea.ImagRange.Maximum));
                        if (isCornerIn != isLastCornerIn)
                        {
                            return true;
                        }
                        return false;
                    });
        }

        private static IEnumerable<Point> GetAllPoints(Size resolution)
        {
            for (int x = 0; x < resolution.Width; x++)
            {
                for (int y = 0; y < resolution.Height; y++)
                {
                    yield return new Point(x, y);
                }
            }
        }
    }
}
