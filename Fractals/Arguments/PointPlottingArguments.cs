using System;

namespace Fractals.Arguments
{
    [Serializable]
    public class PointPlottingArguments
    {
        public int ResolutionWidth { get; set; }

        public int ResolutionHeight { get; set; }

        public string InputDirectory { get; set; }

        public string InputFilePattern { get; set; }
        
        public string OutputDirectory { get; set; }
        
        public string OutputFilename { get; set; }
    }
}