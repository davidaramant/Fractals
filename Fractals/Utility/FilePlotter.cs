using System.Collections.Generic;
using Fractals.Model;

namespace Fractals.Utility
{
    public sealed class FilePlotter : Plotter
    {
        private const int Bailout = 30000;

        private readonly string _inputDirectory;
        private readonly string _inputFilenamePattern;

        public FilePlotter(string inputDirectory, string inputFilenamePattern, string directory, string filename, int width, int height)
            : base(directory, filename, width, height, Bailout)
        {
            _inputDirectory = inputDirectory;
            _inputFilenamePattern = inputFilenamePattern;
        }

        protected override IEnumerable<Complex> GetNumbers()
        {
            var list = new ComplexNumberListReader(_inputDirectory, _inputFilenamePattern);
            return list.GetNumbers();
        }
    }
}
