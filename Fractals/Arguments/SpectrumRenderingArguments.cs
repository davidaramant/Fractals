using System;

namespace Fractals.Arguments
{
    [Serializable]
    public class SpectrumRenderingArguments
    {
        public Resolution Resolution { get; set; }

        public string InputDirectory { get; set; }

        public string InputFilename { get; set; }

        public string OutputDirectory { get; set; }

        public string OutputFilenamePrefix { get; set; }

        public double StartingColor { get; set; }

        public double EndingColor { get; set; }

        public double StepSize { get; set; }
    }
}