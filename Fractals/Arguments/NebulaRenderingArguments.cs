using System;

namespace Fractals.Arguments
{
    [Serializable]
    public class NebulaRenderingArguments
    {
        public Resolution Resolution { get; set; }

        public string InputDirectory { get; set; }

        public string RedInputFilename { get; set; }

        public string GreenInputFilename { get; set; }

        public string BlueInputFilename { get; set; }

        public string OutputDirectory { get; set; }

        public string OutputFilename { get; set; }
    }
}