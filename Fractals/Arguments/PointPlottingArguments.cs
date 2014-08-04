using System;

namespace Fractals.Arguments
{
    [Serializable]
    public class PointPlottingArguments
    {
        public Resolution Resolution { get; set; }

        public string InputDirectory { get; set; }

        public string InputFilePattern { get; set; }
        
        public string OutputDirectory { get; set; }
        
        public string OutputFilename { get; set; }
    }
}