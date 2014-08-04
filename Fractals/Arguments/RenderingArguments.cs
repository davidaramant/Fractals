using System;

namespace Fractals.Arguments
{
    [Serializable]
    public class RenderingArguments
    {
        public int ResolutionWidth { get; set; }
        
        public int ResolutionHeight { get; set; }

        public string InputDirectory { get; set; }

        public string InputFilename { get; set; }

        public string OutputDirectory { get; set; }

        public string OutputFilename { get; set; }
    }
}