using System;

namespace Fractals.Arguments
{
    [Serializable]
    public class ExampleImageRendererArguments
    {
        public int ResolutionWidth { get; set; }

        public int ResolutionHeight { get; set; }

        public string OutputDirectory { get; set; }

        public string OutputFilename { get; set; }
    }
}