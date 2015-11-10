using Fractals.Utility;
using log4net;

namespace Fractals.Renderer
{
    public class SpectrumPlotRenderer
    {
        private readonly string _inputDirectory;
        private readonly string _inputFilename;
        private readonly int _width;
        private readonly int _height;

        private static ILog _log;

        public SpectrumPlotRenderer(string inputDirectory, string inputFilename, int width, int height)
        {
            _inputDirectory = inputDirectory;
            _inputFilename = inputFilename;
            _width = width;
            _height = height;

            _log = LogManager.GetLogger(GetType());
        }

        public void Render(string outputDirectory, string outputFilenamePrefix, double startingColor, double endingColor, double stepSize)
        {
            var renderer = new PlotRenderer(_inputDirectory, _inputFilename, _width, _height);

            for (double current = startingColor; current <= endingColor && current < 100; current += stepSize)
            {
                _log.InfoFormat("Rendering for color: {0}", current);

                var ramp = ColorGradients.GetIntensityGradient(current);
                var outputFilename = string.Format("{0}-{1,3:000.##}", outputFilenamePrefix, current);

                renderer.Render(outputDirectory, outputFilename, ramp);
            }
        }
    }
}