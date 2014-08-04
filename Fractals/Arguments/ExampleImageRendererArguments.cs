using System;

namespace Fractals.Arguments
{
    [Serializable]
    public class ExampleImageRendererArguments
    {
        public Resolution Resolution { get; set; }

        public string OutputDirectory { get; set; }

        public string OutputFilename { get; set; }
    }
}