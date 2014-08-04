using System;

namespace Fractals.Arguments
{
    [Serializable]
    public class EdgeAreaArguments
    {
        public string OutputDirectory { get; set; }

        public string OutputFilename { get; set; }

        public Resolution Resolution { get; set; }

        public double GridSize { get; set; }
    }
}