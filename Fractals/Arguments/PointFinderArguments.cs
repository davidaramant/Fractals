using System;

namespace Fractals.Arguments
{
    [Serializable]
    public class PointFinderArguments
    {
        public string OutputDirectory { get; set; }

        public string OutputFilenamePrefix { get; set; }

        public string InputDirectory { get; set; }

        public string InputEdgeFilename { get; set; }

        public int MinimumThreshold { get; set; }

        public int MaximumThreshold { get; set; }

        public PointSelectionStrategy SelectionStrategy { get; set; }
    }
}
