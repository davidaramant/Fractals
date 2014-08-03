using System.Collections.Generic;
using Fractals.Model;
using log4net;

namespace Fractals.Utility
{
    public sealed class AntiBuddhabrotPlotter : Plotter
    {
        private const int Bailout = 5000;

        private static ILog _log;

        public AntiBuddhabrotPlotter(string directory, string filename, int width, int height)
            : base(directory, filename, width, height, Bailout)
        {
            _log = LogManager.GetLogger(GetType());
        }

        protected override IEnumerable<Complex> GetNumbers()
        {
            _log.InfoFormat("Finding numbers ({0}x{1})", Resolution.Width, Resolution.Height);
            return new MandelbrotFinder().FindPoints(
                Resolution,
                new InclusiveRange(-2, 1),
                new InclusiveRange(-1.5, 1.5));
        }
    }
}