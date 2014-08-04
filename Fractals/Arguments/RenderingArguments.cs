using System;

namespace Fractals.Arguments
{
    [Serializable]
    public class RenderingArguments
    {
        public Resolution Resolution { get; set; }

        public string InputDirectory { get; set; }

        public string InputFilename { get; set; }

        public string OutputDirectory { get; set; }

        public string OutputFilename { get; set; }
    }
}